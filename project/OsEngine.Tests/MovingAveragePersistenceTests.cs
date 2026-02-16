using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class MovingAveragePersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexMovingAverageJson";
        using MovingAverageFileScope scope = new MovingAverageFileScope(name);

        MovingAverage source = new MovingAverage(name, canDelete: true)
        {
            ColorBase = Color.OrangeRed,
            Length = 21,
            PaintOn = false,
            TypeCalculationAverage = MovingAverageTypeCalculation.Adaptive,
            TypePointsToSearch = PriceTypePoints.Median,
            KaufmanFastEma = 4,
            KaufmanSlowEma = 40
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        MovingAverage loaded = new MovingAverage(name, canDelete: true);

        Assert.Equal(Color.OrangeRed.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(21, loaded.Length);
        Assert.False(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Adaptive, loaded.TypeCalculationAverage);
        Assert.Equal(PriceTypePoints.Median, loaded.TypePointsToSearch);
        Assert.Equal(4, loaded.KaufmanFastEma);
        Assert.Equal(40, loaded.KaufmanSlowEma);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexMovingAverageLegacy";
        using MovingAverageFileScope scope = new MovingAverageFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.DarkRed.ToArgb().ToString(),
            "12",
            "True",
            "Exponential",
            "Close",
            "2",
            "30",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        MovingAverage loaded = new MovingAverage(name, canDelete: true);

        Assert.Equal(Color.DarkRed.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(12, loaded.Length);
        Assert.True(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Exponential, loaded.TypeCalculationAverage);
        Assert.Equal(PriceTypePoints.Close, loaded.TypePointsToSearch);
        Assert.Equal(2, loaded.KaufmanFastEma);
        Assert.Equal(30, loaded.KaufmanSlowEma);
    }

    private sealed class MovingAverageFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public MovingAverageFileScope(string name)
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
