#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class ForceIndexPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexForceIndexJson";
        using ForceIndexFileScope scope = new ForceIndexFileScope(name);

        ForceIndex source = new ForceIndex(name, canDelete: true)
        {
            Period = 21,
            TypePoint = PriceTypePoints.High,
            TypeCalculationAverage = MovingAverageTypeCalculation.Weighted,
            ColorBase = Color.DarkKhaki,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ForceIndex loaded = new ForceIndex(name, canDelete: true);

        Assert.Equal(21, loaded.Period);
        Assert.Equal(PriceTypePoints.High, loaded.TypePoint);
        Assert.Equal(MovingAverageTypeCalculation.Weighted, loaded.TypeCalculationAverage);
        Assert.Equal(Color.DarkKhaki.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexForceIndexLegacy";
        using ForceIndexFileScope scope = new ForceIndexFileScope(name);

        string legacy = string.Join(
            "\n",
            "13",
            "Low",
            "Exponential",
            Color.DarkSeaGreen.ToArgb().ToString(),
            "True") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        ForceIndex loaded = new ForceIndex(name, canDelete: true);

        Assert.Equal(13, loaded.Period);
        Assert.Equal(PriceTypePoints.Low, loaded.TypePoint);
        Assert.Equal(MovingAverageTypeCalculation.Exponential, loaded.TypeCalculationAverage);
        Assert.Equal(Color.DarkSeaGreen.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.True(loaded.PaintOn);
    }

    private sealed class ForceIndexFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public ForceIndexFileScope(string name)
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
