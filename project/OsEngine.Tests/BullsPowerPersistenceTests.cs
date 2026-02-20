#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class BullsPowerPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexBullsPowerJson";
        using BullsPowerFileScope scope = new BullsPowerFileScope(name);

        BullsPower source = new BullsPower(name, canDelete: true)
        {
            Period = 19,
            ColorUp = Color.MediumPurple,
            ColorDown = Color.IndianRed,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        BullsPower loaded = new BullsPower(name, canDelete: true);

        Assert.Equal(19, loaded.Period);
        Assert.Equal(Color.MediumPurple.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.IndianRed.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexBullsPowerLegacy";
        using BullsPowerFileScope scope = new BullsPowerFileScope(name);

        string legacy = string.Join(
            "\n",
            "27",
            Color.GreenYellow.ToArgb().ToString(),
            Color.Maroon.ToArgb().ToString(),
            "True") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        BullsPower loaded = new BullsPower(name, canDelete: true);

        Assert.Equal(27, loaded.Period);
        Assert.Equal(Color.GreenYellow.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.Maroon.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.True(loaded.PaintOn);
    }

    private sealed class BullsPowerFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public BullsPowerFileScope(string name)
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
