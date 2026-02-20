#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class TickVolumePersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexTickVolumeJson";
        using TickVolumeFileScope scope = new TickVolumeFileScope(name);

        TickVolume source = new TickVolume(name, canDelete: true)
        {
            ColorUp = Color.LightSkyBlue,
            ColorDown = Color.IndianRed,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        TickVolume loaded = new TickVolume(name, canDelete: true);

        Assert.Equal(Color.LightSkyBlue.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.IndianRed.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexTickVolumeLegacy";
        using TickVolumeFileScope scope = new TickVolumeFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.MediumTurquoise.ToArgb().ToString(),
            Color.SaddleBrown.ToArgb().ToString(),
            "True") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        TickVolume loaded = new TickVolume(name, canDelete: true);

        Assert.Equal(Color.MediumTurquoise.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.SaddleBrown.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.True(loaded.PaintOn);
    }

    private sealed class TickVolumeFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public TickVolumeFileScope(string name)
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
