using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class TradeThreadPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexTradeThreadJson";
        using TradeThreadFileScope scope = new TradeThreadFileScope(name);

        TradeThread source = new TradeThread(name, canDelete: true)
        {
            ColorBase = Color.CadetBlue,
            Length = 41,
            PaintOn = false,
            TypePointsToSearch = PriceTypePoints.Close
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        TradeThread loaded = new TradeThread(name, canDelete: true);

        Assert.Equal(Color.CadetBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(41, loaded.Length);
        Assert.False(loaded.PaintOn);
        Assert.Equal(PriceTypePoints.Close, loaded.TypePointsToSearch);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexTradeThreadLegacy";
        using TradeThreadFileScope scope = new TradeThreadFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.Teal.ToArgb().ToString(),
            "20",
            "True",
            "Typical",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        TradeThread loaded = new TradeThread(name, canDelete: true);

        Assert.Equal(Color.Teal.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(20, loaded.Length);
        Assert.True(loaded.PaintOn);
        Assert.Equal(PriceTypePoints.Typical, loaded.TypePointsToSearch);
    }

    private sealed class TradeThreadFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public TradeThreadFileScope(string name)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, name + ".txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
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

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }
    }
}
