#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Market.Servers.InteractiveBrokers;
using Xunit;

namespace OsEngine.Tests;

public class InteractiveBrokersSecuritiesPersistenceTests
{
    [Fact]
    public void SaveIbSecurities_ShouldPersistJson_AndLoadRoundTrip()
    {
        using IbSecuritiesFileScope scope = new IbSecuritiesFileScope();

        InteractiveBrokersServerRealization writer = new InteractiveBrokersServerRealization();
        List<SecurityIb> source = new List<SecurityIb>
        {
            new SecurityIb
            {
                Symbol = "AAPL",
                LocalSymbol = "AAPL",
                Currency = "USD",
                Exchange = "SMART",
                PrimaryExch = "NASDAQ",
                SecType = "STK",
                Strike = 0,
                TradingClass = "NMS",
                ConId = 123,
                CreateMarketDepthFromTrades = true
            },
            new SecurityIb
            {
                Symbol = "EUR",
                LocalSymbol = "EUR.USD",
                Currency = "USD",
                Exchange = "IDEALPRO",
                PrimaryExch = "IDEALPRO",
                SecType = "CASH",
                Strike = 0,
                TradingClass = "CASH",
                ConId = 456,
                CreateMarketDepthFromTrades = false
            }
        };
        SetSecurities(writer, source);
        writer.SaveIbSecurities();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        InteractiveBrokersServerRealization reader = new InteractiveBrokersServerRealization();
        List<SecurityIb> loaded = GetSecurities(reader);

        Assert.Equal(2, loaded.Count);
        Assert.Equal("AAPL", loaded[0].Symbol);
        Assert.Equal("SMART", loaded[0].Exchange);
        Assert.Equal(123, loaded[0].ConId);
        Assert.True(loaded[0].CreateMarketDepthFromTrades);

        Assert.Equal("EUR.USD", loaded[1].LocalSymbol);
        Assert.Equal("CASH", loaded[1].SecType);
        Assert.Equal(456, loaded[1].ConId);
        Assert.False(loaded[1].CreateMarketDepthFromTrades);
    }

    [Fact]
    public void LoadIbSecurities_ShouldSupportLegacyLineBasedFormat()
    {
        using IbSecuritiesFileScope scope = new IbSecuritiesFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "desc@321@USD@SMART@@False@AAPL@@NASDAQ@@@@STK@0@AAPL@NMS@True@",
            "desc2@654@USD@IDEALPRO@@False@EUR.USD@@IDEALPRO@@@@CASH@0@EUR@CASH@False@"
        });

        InteractiveBrokersServerRealization reader = new InteractiveBrokersServerRealization();
        List<SecurityIb> loaded = GetSecurities(reader);

        Assert.Equal(2, loaded.Count);
        Assert.Equal("AAPL", loaded[0].Symbol);
        Assert.Equal("SMART", loaded[0].Exchange);
        Assert.Equal(321, loaded[0].ConId);
        Assert.True(loaded[0].CreateMarketDepthFromTrades);

        Assert.Equal("EUR", loaded[1].Symbol);
        Assert.Equal("IDEALPRO", loaded[1].Exchange);
        Assert.Equal(654, loaded[1].ConId);
        Assert.False(loaded[1].CreateMarketDepthFromTrades);
    }

    private static List<SecurityIb> GetSecurities(InteractiveBrokersServerRealization realization)
    {
        FieldInfo field = typeof(InteractiveBrokersServerRealization)
            .GetField("_secIB", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field _secIB not found.");
        return (List<SecurityIb>)field.GetValue(realization)!;
    }

    private static void SetSecurities(InteractiveBrokersServerRealization realization, List<SecurityIb> securities)
    {
        FieldInfo field = typeof(InteractiveBrokersServerRealization)
            .GetField("_secIB", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field _secIB not found.");
        field.SetValue(realization, securities);
    }

    private sealed class IbSecuritiesFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public IbSecuritiesFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "IbSecuritiesToWatch.txt");
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
