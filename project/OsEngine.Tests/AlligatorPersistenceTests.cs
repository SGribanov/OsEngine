#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class AlligatorPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexAlligatorJson";
        using AlligatorFileScope scope = new AlligatorFileScope(name);

        Alligator source = new Alligator(name, canDelete: true)
        {
            LengthBase = 21,
            ShiftBase = 8,
            ColorBase = Color.SandyBrown,
            LengthUp = 13,
            ShiftUp = 5,
            ColorUp = Color.SeaGreen,
            LengthDown = 34,
            ShiftDown = 13,
            ColorDown = Color.SlateBlue,
            PaintOn = false,
            TypeCalculationAverage = MovingAverageTypeCalculation.Weighted
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        Alligator loaded = new Alligator(name, canDelete: true);

        Assert.Equal(21, loaded.LengthBase);
        Assert.Equal(8, loaded.ShiftBase);
        Assert.Equal(Color.SandyBrown.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(13, loaded.LengthUp);
        Assert.Equal(5, loaded.ShiftUp);
        Assert.Equal(Color.SeaGreen.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(34, loaded.LengthDown);
        Assert.Equal(13, loaded.ShiftDown);
        Assert.Equal(Color.SlateBlue.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.False(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Weighted, loaded.TypeCalculationAverage);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexAlligatorLegacy";
        using AlligatorFileScope scope = new AlligatorFileScope(name);

        string legacy = string.Join(
            "\n",
            "18",
            "7",
            Color.DarkRed.ToArgb().ToString(),
            "11",
            "4",
            Color.Green.ToArgb().ToString(),
            "29",
            "12",
            Color.RoyalBlue.ToArgb().ToString(),
            "True",
            "Simple") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        Alligator loaded = new Alligator(name, canDelete: true);

        Assert.Equal(18, loaded.LengthBase);
        Assert.Equal(7, loaded.ShiftBase);
        Assert.Equal(Color.DarkRed.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(11, loaded.LengthUp);
        Assert.Equal(4, loaded.ShiftUp);
        Assert.Equal(Color.Green.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(29, loaded.LengthDown);
        Assert.Equal(12, loaded.ShiftDown);
        Assert.Equal(Color.RoyalBlue.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.True(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Simple, loaded.TypeCalculationAverage);
    }

    private sealed class AlligatorFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public AlligatorFileScope(string name)
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
