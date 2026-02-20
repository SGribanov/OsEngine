#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class StochasticOscillatorPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexStochasticOscillatorJson";
        using StochasticOscillatorFileScope scope = new StochasticOscillatorFileScope(name);

        StochasticOscillator source = new StochasticOscillator(name, canDelete: true)
        {
            ColorUp = Color.DodgerBlue,
            ColorDown = Color.Firebrick,
            PaintOn = false,
            TypeCalculationAverage = MovingAverageTypeCalculation.Weighted
        };
        source.P1 = 7;
        source.P2 = 4;
        source.P3 = 5;
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        StochasticOscillator loaded = new StochasticOscillator(name, canDelete: true);

        Assert.Equal(7, loaded.P1);
        Assert.Equal(4, loaded.P2);
        Assert.Equal(5, loaded.P3);
        Assert.Equal(Color.DodgerBlue.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.Firebrick.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.False(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Simple, loaded.TypeCalculationAverage);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_WithSaveOrdering()
    {
        const string name = "CodexStochasticOscillatorLegacy";
        using StochasticOscillatorFileScope scope = new StochasticOscillatorFileScope(name);

        string legacy = string.Join(
            "\n",
            "9",
            "3",
            "4",
            "Simple",
            string.Empty,
            Color.DarkCyan.ToArgb().ToString(),
            Color.DarkMagenta.ToArgb().ToString(),
            "True") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        StochasticOscillator loaded = new StochasticOscillator(name, canDelete: true);

        Assert.Equal(9, loaded.P1);
        Assert.Equal(3, loaded.P2);
        Assert.Equal(4, loaded.P3);
        Assert.Equal(MovingAverageTypeCalculation.Simple, loaded.TypeCalculationAverage);
        Assert.Equal(Color.DarkCyan.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.DarkMagenta.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.True(loaded.PaintOn);
    }

    private sealed class StochasticOscillatorFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public StochasticOscillatorFileScope(string name)
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
