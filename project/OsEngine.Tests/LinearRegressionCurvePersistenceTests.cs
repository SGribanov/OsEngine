using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class LinearRegressionCurvePersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexLinearRegressionCurveJson";
        using LinearRegressionCurveFileScope scope = new LinearRegressionCurveFileScope(name);

        LinearRegressionCurve source = new LinearRegressionCurve(name, canDelete: true)
        {
            ColorBase = Color.CadetBlue,
            Length = 44,
            Lag = 3,
            PaintOn = false,
            TypePointsToSearch = PriceTypePoints.Typical
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        LinearRegressionCurve loaded = new LinearRegressionCurve(name, canDelete: true);

        Assert.Equal(Color.CadetBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(44, loaded.Length);
        Assert.Equal(3, loaded.Lag);
        Assert.False(loaded.PaintOn);
        Assert.Equal(PriceTypePoints.Typical, loaded.TypePointsToSearch);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_WithOptionalTrailingLine()
    {
        const string name = "CodexLinearRegressionCurveLegacy";
        using LinearRegressionCurveFileScope scope = new LinearRegressionCurveFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.DeepSkyBlue.ToArgb().ToString(),
            "30",
            "1",
            "True",
            "Close",
            "ignored_legacy_trailing_line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        LinearRegressionCurve loaded = new LinearRegressionCurve(name, canDelete: true);

        Assert.Equal(Color.DeepSkyBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(30, loaded.Length);
        Assert.Equal(1, loaded.Lag);
        Assert.True(loaded.PaintOn);
        Assert.Equal(PriceTypePoints.Close, loaded.TypePointsToSearch);
    }

    private sealed class LinearRegressionCurveFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public LinearRegressionCurveFileScope(string name)
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
