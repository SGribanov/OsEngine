#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class AwesomeOscillatorPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexAwesomeOscillatorJson";
        using AwesomeOscillatorFileScope scope = new AwesomeOscillatorFileScope(name);

        AwesomeOscillator source = new AwesomeOscillator(name, canDelete: true)
        {
            ColorUp = Color.DeepSkyBlue,
            ColorDown = Color.Firebrick,
            LengthShort = 7,
            LengthLong = 39,
            PaintOn = false,
            TypeCalculationAverage = MovingAverageTypeCalculation.Weighted
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        AwesomeOscillator loaded = new AwesomeOscillator(name, canDelete: true);

        Assert.Equal(Color.DeepSkyBlue.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.Firebrick.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.Equal(7, loaded.LengthShort);
        Assert.Equal(39, loaded.LengthLong);
        Assert.False(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Weighted, loaded.TypeCalculationAverage);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexAwesomeOscillatorLegacy";
        using AwesomeOscillatorFileScope scope = new AwesomeOscillatorFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.DodgerBlue.ToArgb().ToString(),
            Color.DarkRed.ToArgb().ToString(),
            "9",
            "45",
            "True",
            "Simple") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        AwesomeOscillator loaded = new AwesomeOscillator(name, canDelete: true);

        Assert.Equal(Color.DodgerBlue.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.DarkRed.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.Equal(9, loaded.LengthShort);
        Assert.Equal(45, loaded.LengthLong);
        Assert.True(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Simple, loaded.TypeCalculationAverage);
    }

    private sealed class AwesomeOscillatorFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public AwesomeOscillatorFileScope(string name)
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
