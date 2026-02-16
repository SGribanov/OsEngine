using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class UltimateOscillatorPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexUltimateOscillatorJson";
        using UltimateOscillatorFileScope scope = new UltimateOscillatorFileScope(name);

        UltimateOscillator source = new UltimateOscillator(name, canDelete: true)
        {
            ColorBase = Color.DarkOrange,
            Period1 = 6,
            Period2 = 12,
            Period3 = 24,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        UltimateOscillator loaded = new UltimateOscillator(name, canDelete: true);

        Assert.Equal(Color.DarkOrange.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(6, loaded.Period1);
        Assert.Equal(12, loaded.Period2);
        Assert.Equal(24, loaded.Period3);
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexUltimateOscillatorLegacy";
        using UltimateOscillatorFileScope scope = new UltimateOscillatorFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.DarkCyan.ToArgb().ToString(),
            "7",
            "14",
            "28",
            "True",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        UltimateOscillator loaded = new UltimateOscillator(name, canDelete: true);

        Assert.Equal(Color.DarkCyan.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(7, loaded.Period1);
        Assert.Equal(14, loaded.Period2);
        Assert.Equal(28, loaded.Period3);
        Assert.True(loaded.PaintOn);
    }

    private sealed class UltimateOscillatorFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public UltimateOscillatorFileScope(string name)
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
