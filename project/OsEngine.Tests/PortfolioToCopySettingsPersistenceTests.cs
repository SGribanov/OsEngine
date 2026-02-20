#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using OsEngine.Market.AutoFollow;
using Xunit;

namespace OsEngine.Tests;

public class PortfolioToCopySettingsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using PortfolioToCopyFileScope scope = new PortfolioToCopyFileScope("CodexPortfolioSettings");

        PortfolioToCopy source = scope.CreateWithoutConstructor();
        scope.SetupForSave(source);
        source.ServerName = "QuikLua_1";
        source.PortfolioName = "TEST_PORTFOLIO";
        source.IsOn = true;
        source.VolumeType = CopyTraderVolumeType.DepoProportional;
        source.VolumeMult = 2.5m;
        source.MasterAsset = "USDT";
        source.SlaveAsset = "RUB";
        source.OrderType = CopyTraderOrdersType.Iceberg;
        source.IcebergCount = 7;
        source.PanelsPosition = "2,2,2";
        source.MinCurrencyQty = 11.2m;
        source.FailOpenOrdersReactionIsOn = false;
        source.FailOpenOrdersCountToReaction = 6;
        source.IcebergMillisecondsDelay = 1500;
        source.SecurityToCopy.Add(new SecurityToCopy
        {
            MasterSecurityName = "SBER",
            MasterSecurityClass = "TQBR",
            SlaveSecurityName = "SBERP",
            SlaveSecurityClass = "TQBR"
        });

        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        PortfolioToCopy target = scope.CreateWithoutConstructor();
        scope.SetupForLoad(target);
        target.Load();

        Assert.Equal("QuikLua_1", target.ServerName);
        Assert.Equal("TEST_PORTFOLIO", target.PortfolioName);
        Assert.True(target.IsOn);
        Assert.Equal(CopyTraderVolumeType.DepoProportional, target.VolumeType);
        Assert.Equal(2.5m, target.VolumeMult);
        Assert.Equal("USDT", target.MasterAsset);
        Assert.Equal("RUB", target.SlaveAsset);
        Assert.Equal(CopyTraderOrdersType.Iceberg, target.OrderType);
        Assert.Equal(7, target.IcebergCount);
        Assert.Equal("2,2,2", target.PanelsPosition);
        Assert.Equal(11.2m, target.MinCurrencyQty);
        Assert.False(target.FailOpenOrdersReactionIsOn);
        Assert.Equal(6, target.FailOpenOrdersCountToReaction);
        Assert.Equal(1500, target.IcebergMillisecondsDelay);
        Assert.Single(target.SecurityToCopy);
        Assert.Equal("SBER", target.SecurityToCopy[0].MasterSecurityName);
        Assert.Equal("SBERP", target.SecurityToCopy[0].SlaveSecurityName);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using PortfolioToCopyFileScope scope = new PortfolioToCopyFileScope("CodexPortfolioLegacy");

        string securities = "GAZP^TQBR^GAZP^TQBR^*";
        string legacyContent = string.Join(
            Environment.NewLine,
            "MetaTrader5_1",
            "LEGACY_PORTFOLIO",
            "False",
            "Simple",
            "3",
            "Prime",
            "Prime",
            "Market",
            "2",
            securities,
            "1,1,1",
            "0",
            "True",
            "10",
            "2000");
        File.WriteAllText(scope.SettingsPath, legacyContent);

        PortfolioToCopy target = scope.CreateWithoutConstructor();
        scope.SetupForLoad(target);
        target.Load();

        Assert.Equal("MetaTrader5_1", target.ServerName);
        Assert.Equal("LEGACY_PORTFOLIO", target.PortfolioName);
        Assert.False(target.IsOn);
        Assert.Equal(CopyTraderVolumeType.Simple, target.VolumeType);
        Assert.Equal(3m, target.VolumeMult);
        Assert.Equal("Prime", target.MasterAsset);
        Assert.Equal("Prime", target.SlaveAsset);
        Assert.Equal(CopyTraderOrdersType.Market, target.OrderType);
        Assert.Equal(2, target.IcebergCount);
        Assert.Equal("1,1,1", target.PanelsPosition);
        Assert.Equal(0m, target.MinCurrencyQty);
        Assert.True(target.FailOpenOrdersReactionIsOn);
        Assert.Equal(10, target.FailOpenOrdersCountToReaction);
        Assert.Equal(2000, target.IcebergMillisecondsDelay);
        Assert.Single(target.SecurityToCopy);
        Assert.Equal("GAZP", target.SecurityToCopy[0].MasterSecurityName);
    }

    private sealed class PortfolioToCopyFileScope : IDisposable
    {
        private readonly string _nameUnique;
        private readonly string _copyTraderDirPath;
        private readonly bool _copyTraderDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public PortfolioToCopyFileScope(string nameUnique)
        {
            _nameUnique = nameUnique;
            _copyTraderDirPath = Path.GetFullPath(Path.Combine("Engine", "CopyTrader"));
            SettingsPath = Path.Combine(_copyTraderDirPath, $"{_nameUnique}.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _copyTraderDirExisted = Directory.Exists(_copyTraderDirPath);
            if (!_copyTraderDirExisted)
            {
                Directory.CreateDirectory(_copyTraderDirPath);
            }

            _settingsFileExisted = File.Exists(SettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(SettingsPath, _settingsBackupPath, overwrite: true);
            }
            else if (File.Exists(_settingsBackupPath))
            {
                File.Delete(_settingsBackupPath);
            }
        }

        public string SettingsPath { get; }

        public PortfolioToCopy CreateWithoutConstructor()
        {
            return (PortfolioToCopy)RuntimeHelpers.GetUninitializedObject(typeof(PortfolioToCopy));
        }

        public void SetupForSave(PortfolioToCopy portfolio)
        {
            portfolio.NameUnique = _nameUnique;
            portfolio.SecurityToCopy = new List<SecurityToCopy>();
        }

        public void SetupForLoad(PortfolioToCopy portfolio)
        {
            portfolio.NameUnique = _nameUnique;
            portfolio.SecurityToCopy = new List<SecurityToCopy>();
        }

        public void Dispose()
        {
            if (_settingsFileExisted)
            {
                if (File.Exists(_settingsBackupPath))
                {
                    File.Copy(_settingsBackupPath, SettingsPath, overwrite: true);
                    File.Delete(_settingsBackupPath);
                }
            }
            else
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }

                if (File.Exists(_settingsBackupPath))
                {
                    File.Delete(_settingsBackupPath);
                }
            }

            if (!_copyTraderDirExisted
                && Directory.Exists(_copyTraderDirPath)
                && !Directory.EnumerateFileSystemEntries(_copyTraderDirPath).Any())
            {
                Directory.Delete(_copyTraderDirPath);
            }
        }
    }
}
