using System;
using System.IO;
using System.Linq;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class TimeFrameBuilderPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexTfBuilderJson";
        using TimeFrameBuilderFileScope scope = new TimeFrameBuilderFileScope(name);

        TimeFrameBuilder source = new TimeFrameBuilder(name, StartProgram.IsOsTrader);
        source.TimeFrame = TimeFrame.Min15;
        source.SaveTradesInCandles = true;
        source.CandleMarketDataType = CandleMarketDataType.MarketDepth;
        source.MarketDepthBuildMaxSpreadIsOn = true;
        source.MarketDepthBuildMaxSpread = 1.25m;
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        TimeFrameBuilder loaded = new TimeFrameBuilder(name, StartProgram.IsOsTrader);
        Assert.Equal(TimeFrame.Min15, loaded.TimeFrame);
        Assert.True(loaded.SaveTradesInCandles);
        Assert.Equal(CandleMarketDataType.MarketDepth, loaded.CandleMarketDataType);
        Assert.True(loaded.MarketDepthBuildMaxSpreadIsOn);
        Assert.Equal(1.25m, loaded.MarketDepthBuildMaxSpread);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexTfBuilderLegacy";
        using TimeFrameBuilderFileScope scope = new TimeFrameBuilderFileScope(name);

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "Min5",
            "True",
            "MarketDepth",
            "Simple",
            string.Empty,
            "True",
            "0.75"
        });

        TimeFrameBuilder loaded = new TimeFrameBuilder(name, StartProgram.IsOsTrader);
        Assert.Equal(TimeFrame.Min5, loaded.TimeFrame);
        Assert.True(loaded.SaveTradesInCandles);
        Assert.Equal(CandleMarketDataType.MarketDepth, loaded.CandleMarketDataType);
        Assert.True(loaded.MarketDepthBuildMaxSpreadIsOn);
        Assert.Equal(0.75m, loaded.MarketDepthBuildMaxSpread);
    }

    private sealed class TimeFrameBuilderFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public TimeFrameBuilderFileScope(string name)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, name + "TimeFrameBuilder.txt");
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
