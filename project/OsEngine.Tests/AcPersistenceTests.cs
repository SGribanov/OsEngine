using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class AcPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexAcJson";
        using AcFileScope scope = new AcFileScope(name);

        Ac source = new Ac(name, canDelete: true)
        {
            ColorUp = Color.LightSeaGreen,
            ColorDown = Color.DarkRed,
            LengthLong = 55,
            LengthShort = 8,
            PaintOn = false,
            TypeCalculationAverage = MovingAverageTypeCalculation.Weighted
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        Ac loaded = new Ac(name, canDelete: true);

        Assert.Equal(Color.LightSeaGreen.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.DarkRed.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.Equal(55, loaded.LengthLong);
        Assert.Equal(8, loaded.LengthShort);
        Assert.False(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Weighted, loaded.TypeCalculationAverage);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexAcLegacy";
        using AcFileScope scope = new AcFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.DeepSkyBlue.ToArgb().ToString(),
            Color.IndianRed.ToArgb().ToString(),
            "34",
            "5",
            "True",
            "Simple",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        Ac loaded = new Ac(name, canDelete: true);

        Assert.Equal(Color.DeepSkyBlue.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.IndianRed.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.Equal(34, loaded.LengthLong);
        Assert.Equal(5, loaded.LengthShort);
        Assert.True(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Simple, loaded.TypeCalculationAverage);
    }

    private sealed class AcFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public AcFileScope(string name)
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
