using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class KalmanFilterPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexKalmanFilterJson";
        using KalmanFilterFileScope scope = new KalmanFilterFileScope(name);

        KalmanFilter source = new KalmanFilter(name, canDelete: true)
        {
            ColorBase = Color.RoyalBlue,
            Sharpness = 2.5m,
            K = 0.7m,
            PaintOn = false,
            TypeCalculationAverage = MovingAverageTypeCalculation.Exponential
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        KalmanFilter loaded = new KalmanFilter(name, canDelete: true);

        Assert.Equal(Color.RoyalBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(2.5m, loaded.Sharpness);
        Assert.Equal(0.7m, loaded.K);
        Assert.False(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Exponential, loaded.TypeCalculationAverage);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexKalmanFilterLegacy";
        using KalmanFilterFileScope scope = new KalmanFilterFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.SteelBlue.ToArgb().ToString(),
            "1.1",
            "3.2",
            "True",
            "Simple",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        KalmanFilter loaded = new KalmanFilter(name, canDelete: true);

        Assert.Equal(Color.SteelBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(1.1m, loaded.Sharpness);
        Assert.Equal(3.2m, loaded.K);
        Assert.True(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Simple, loaded.TypeCalculationAverage);
    }

    private sealed class KalmanFilterFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public KalmanFilterFileScope(string name)
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
