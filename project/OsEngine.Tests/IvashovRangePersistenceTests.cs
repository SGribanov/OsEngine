#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class IvashovRangePersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexIvashovRangeJson";
        using IvashovRangeFileScope scope = new IvashovRangeFileScope(name);

        IvashovRange source = new IvashovRange(name, canDelete: true)
        {
            ColorBase = Color.DarkSeaGreen,
            LengthMa = 55,
            LengthAverage = 21,
            PaintOn = false,
            TypeCalculationAverage = MovingAverageTypeCalculation.Weighted
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        IvashovRange loaded = new IvashovRange(name, canDelete: true);

        Assert.Equal(Color.DarkSeaGreen.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(55, loaded.LengthMa);
        Assert.Equal(21, loaded.LengthAverage);
        Assert.False(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Weighted, loaded.TypeCalculationAverage);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_WithOptionalLengthAverageMissing()
    {
        const string name = "CodexIvashovRangeLegacy";
        using IvashovRangeFileScope scope = new IvashovRangeFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.DarkKhaki.ToArgb().ToString(),
            "34",
            "True",
            "Simple") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        IvashovRange loaded = new IvashovRange(name, canDelete: true);

        Assert.Equal(Color.DarkKhaki.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(34, loaded.LengthMa);
        Assert.Equal(34, loaded.LengthAverage);
        Assert.True(loaded.PaintOn);
        Assert.Equal(MovingAverageTypeCalculation.Simple, loaded.TypeCalculationAverage);
    }

    private sealed class IvashovRangeFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public IvashovRangeFileScope(string name)
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
