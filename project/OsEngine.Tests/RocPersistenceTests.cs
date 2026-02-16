using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class RocPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexRocJson";
        using RocFileScope scope = new RocFileScope(name);

        Roc source = new Roc(name, canDelete: true)
        {
            Period = 29,
            TypePoint = PriceTypePoints.Open,
            ColorBase = Color.OliveDrab,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        Roc loaded = new Roc(name, canDelete: true);

        Assert.Equal(29, loaded.Period);
        Assert.Equal(PriceTypePoints.Open, loaded.TypePoint);
        Assert.Equal(Color.OliveDrab.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexRocLegacy";
        using RocFileScope scope = new RocFileScope(name);

        string legacy = string.Join(
            "\n",
            "14",
            "Low",
            Color.CadetBlue.ToArgb().ToString(),
            "True",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        Roc loaded = new Roc(name, canDelete: true);

        Assert.Equal(14, loaded.Period);
        Assert.Equal(PriceTypePoints.Low, loaded.TypePoint);
        Assert.Equal(Color.CadetBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.True(loaded.PaintOn);
    }

    private sealed class RocFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public RocFileScope(string name)
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
