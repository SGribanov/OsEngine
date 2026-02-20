#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class EfficiencyRatioPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexEfficiencyRatioJson";
        using EfficiencyRatioFileScope scope = new EfficiencyRatioFileScope(name);

        EfficiencyRatio source = new EfficiencyRatio(name, canDelete: true)
        {
            ColorBase = Color.DarkSlateBlue,
            Length = 27,
            PaintOn = false,
            TypeCalculationAverage = MovingAverageTypeCalculation.Weighted
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        EfficiencyRatio loaded = new EfficiencyRatio(name, canDelete: true);

        Assert.Equal(Color.DarkSlateBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(27, loaded.Length);
        Assert.False(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Weighted, loaded.TypeCalculationAverage);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexEfficiencyRatioLegacy";
        using EfficiencyRatioFileScope scope = new EfficiencyRatioFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.DarkViolet.ToArgb().ToString(),
            "11",
            "True",
            "Exponential") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        EfficiencyRatio loaded = new EfficiencyRatio(name, canDelete: true);

        Assert.Equal(Color.DarkViolet.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(11, loaded.Length);
        Assert.True(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Exponential, loaded.TypeCalculationAverage);
    }

    private sealed class EfficiencyRatioFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public EfficiencyRatioFileScope(string name)
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
