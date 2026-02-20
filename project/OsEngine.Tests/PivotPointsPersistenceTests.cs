#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class PivotPointsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexPivotPointsJson";
        using PivotPointsFileScope scope = new PivotPointsFileScope(name);

        PivotPoints source = new PivotPoints(name, canDelete: true)
        {
            ColorP = Color.Gold,
            ColorS1 = Color.Firebrick,
            ColorS2 = Color.IndianRed,
            ColorS3 = Color.Maroon,
            ColorR1 = Color.DodgerBlue,
            ColorR2 = Color.DeepSkyBlue,
            ColorR3 = Color.CadetBlue,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        PivotPoints loaded = new PivotPoints(name, canDelete: true);

        Assert.Equal(Color.Gold.ToArgb(), loaded.ColorP.ToArgb());
        Assert.Equal(Color.Firebrick.ToArgb(), loaded.ColorS1.ToArgb());
        Assert.Equal(Color.IndianRed.ToArgb(), loaded.ColorS2.ToArgb());
        Assert.Equal(Color.Maroon.ToArgb(), loaded.ColorS3.ToArgb());
        Assert.Equal(Color.DodgerBlue.ToArgb(), loaded.ColorR1.ToArgb());
        Assert.Equal(Color.DeepSkyBlue.ToArgb(), loaded.ColorR2.ToArgb());
        Assert.Equal(Color.CadetBlue.ToArgb(), loaded.ColorR3.ToArgb());
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_WithColorStrings()
    {
        const string name = "CodexPivotPointsLegacy";
        using PivotPointsFileScope scope = new PivotPointsFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.LawnGreen.ToString(),
            Color.DarkRed.ToString(),
            Color.IndianRed.ToString(),
            Color.Maroon.ToString(),
            Color.DodgerBlue.ToString(),
            Color.DeepSkyBlue.ToString(),
            Color.CadetBlue.ToString(),
            "True",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        PivotPoints loaded = new PivotPoints(name, canDelete: true);

        Assert.Equal(Color.LawnGreen.ToArgb(), loaded.ColorP.ToArgb());
        Assert.Equal(Color.DarkRed.ToArgb(), loaded.ColorS1.ToArgb());
        Assert.Equal(Color.IndianRed.ToArgb(), loaded.ColorS2.ToArgb());
        Assert.Equal(Color.Maroon.ToArgb(), loaded.ColorS3.ToArgb());
        Assert.Equal(Color.DodgerBlue.ToArgb(), loaded.ColorR1.ToArgb());
        Assert.Equal(Color.DeepSkyBlue.ToArgb(), loaded.ColorR2.ToArgb());
        Assert.Equal(Color.CadetBlue.ToArgb(), loaded.ColorR3.ToArgb());
        Assert.True(loaded.PaintOn);
    }

    private sealed class PivotPointsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public PivotPointsFileScope(string name)
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
