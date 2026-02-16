using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Market.Servers.Atp;
using Xunit;

namespace OsEngine.Tests;

public class AtpServerSecuritiesPersistenceTests
{
    [Fact]
    public void SaveSecurities_ShouldPersistJson_AndLoadRoundTrip()
    {
        using AtpSecuritiesFileScope scope = new AtpSecuritiesFileScope();

        AtpServerRealization source = scope.CreateWithoutConstructor();
        source._securities = new List<Security>
        {
            new Security
            {
                Name = "SBER",
                NameClass = "legacy",
                NameFull = "Sberbank",
                NameId = "SBER_ID",
                State = SecurityStateType.Activ,
                PriceStep = 0.01m,
                Lot = 10,
                PriceStepCost = 0.01m,
                MarginBuy = 1000,
                SecurityType = SecurityType.Stock,
                Decimals = 2,
                PriceLimitLow = 100,
                PriceLimitHigh = 200,
                OptionType = OptionType.Call,
                Strike = 150,
                Expiration = new DateTime(2025, 1, 2, 3, 4, 5)
            }
        };

        source.TrySaveSecuritiesInFile();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        AtpServerRealization loaded = scope.CreateWithoutConstructor();
        loaded._securities = new List<Security>();
        loaded.TryLoadSecuritiesFromFile();

        Assert.Single(loaded._securities);
        Security security = loaded._securities[0];
        Assert.Equal("SBER", security.Name);
        Assert.Equal("atpSecurity", security.NameClass);
        Assert.Equal("SBER_ID", security.NameId);
        Assert.Equal(SecurityType.Stock, security.SecurityType);
        Assert.Equal(2, security.Decimals);
        Assert.Equal(new DateTime(2025, 1, 2, 3, 4, 5), security.Expiration);
    }

    [Fact]
    public void LoadSecurities_ShouldSupportLegacyLineBasedFormat()
    {
        using AtpSecuritiesFileScope scope = new AtpSecuritiesFileScope();

        string legacyLine = string.Join("!", new[]
        {
            "GAZP",
            "atpSecurity",
            "LegacyNameFull",
            "",
            "LegacyNameFullDisplay",
            "Activ",
            "0.01",
            "1",
            "0.01",
            "10",
            "Stock",
            "2",
            "100",
            "200",
            "Call",
            "150",
            "2025-01-02T03:04:05"
        }) + "!";
        File.WriteAllLines(scope.SettingsPath, new[] { legacyLine });

        AtpServerRealization loaded = scope.CreateWithoutConstructor();
        loaded._securities = new List<Security>();
        loaded.TryLoadSecuritiesFromFile();

        Assert.Single(loaded._securities);
        Security security = loaded._securities[0];
        Assert.Equal("GAZP", security.Name);
        Assert.Equal("LegacyNameFullDisplay", security.NameFull);
        Assert.Equal("LegacyNameFull", security.NameId);
        Assert.Equal(SecurityType.Stock, security.SecurityType);
        Assert.Equal(2, security.Decimals);
    }

    private sealed class AtpSecuritiesFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public AtpSecuritiesFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "AtpSecurities.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _settingsFileExisted = File.Exists(SettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(SettingsPath, _settingsBackup, overwrite: true);
            }
            else if (File.Exists(_settingsBackup))
            {
                File.Delete(_settingsBackup);
            }
        }

        public string SettingsPath { get; }

        public AtpServerRealization CreateWithoutConstructor()
        {
            return (AtpServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(AtpServerRealization));
        }

        public void Dispose()
        {
            if (_settingsFileExisted)
            {
                if (File.Exists(_settingsBackup))
                {
                    File.Copy(_settingsBackup, SettingsPath, overwrite: true);
                    File.Delete(_settingsBackup);
                }
            }
            else
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }

                if (File.Exists(_settingsBackup))
                {
                    File.Delete(_settingsBackup);
                }
            }

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }
    }
}
