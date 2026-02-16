using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class ParabolicSarPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexParabolicSarJson";
        using ParabolicSarFileScope scope = new ParabolicSarFileScope(name);

        ParabolicSaR source = new ParabolicSaR(name, canDelete: true)
        {
            ColorUp = Color.SpringGreen,
            ColorDown = Color.IndianRed,
            Af = 0.03d,
            MaxAf = 0.35d,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ParabolicSaR loaded = new ParabolicSaR(name, canDelete: true);

        Assert.Equal(Color.SpringGreen.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.IndianRed.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.Equal(0.03d, loaded.Af, 8);
        Assert.Equal(0.35d, loaded.MaxAf, 8);
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_WithOptionalTrailingLine()
    {
        const string name = "CodexParabolicSarLegacy";
        using ParabolicSarFileScope scope = new ParabolicSarFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.Green.ToArgb().ToString(),
            Color.Red.ToArgb().ToString(),
            "0.02",
            "0.2",
            "True",
            "ignored_legacy_trailing_line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        ParabolicSaR loaded = new ParabolicSaR(name, canDelete: true);

        Assert.Equal(Color.Green.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.Red.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.Equal(0.02d, loaded.Af, 8);
        Assert.Equal(0.2d, loaded.MaxAf, 8);
        Assert.True(loaded.PaintOn);
    }

    private sealed class ParabolicSarFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public ParabolicSarFileScope(string name)
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
