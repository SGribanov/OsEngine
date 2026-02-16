using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class VolumeOscillatorPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexVolumeOscillatorJson";
        using VolumeOscillatorFileScope scope = new VolumeOscillatorFileScope(name);

        VolumeOscillator source = new VolumeOscillator(name, canDelete: true)
        {
            ColorBase = Color.MediumSlateBlue,
            Length1 = 34,
            Length2 = 13,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        VolumeOscillator loaded = new VolumeOscillator(name, canDelete: true);

        Assert.Equal(Color.MediumSlateBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(34, loaded.Length1);
        Assert.Equal(13, loaded.Length2);
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexVolumeOscillatorLegacy";
        using VolumeOscillatorFileScope scope = new VolumeOscillatorFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.SteelBlue.ToArgb().ToString(),
            "55",
            "21",
            "True",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        VolumeOscillator loaded = new VolumeOscillator(name, canDelete: true);

        Assert.Equal(Color.SteelBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(55, loaded.Length1);
        Assert.Equal(21, loaded.Length2);
        Assert.True(loaded.PaintOn);
    }

    private sealed class VolumeOscillatorFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public VolumeOscillatorFileScope(string name)
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
