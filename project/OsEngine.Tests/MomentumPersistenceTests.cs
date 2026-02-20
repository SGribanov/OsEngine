#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class MomentumPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexMomentumJson";
        using MomentumFileScope scope = new MomentumFileScope(name);

        Momentum source = new Momentum(name, canDelete: true)
        {
            Nperiod = 17,
            TypePoint = PriceTypePoints.High,
            ColorBase = Color.MediumVioletRed,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        Momentum loaded = new Momentum(name, canDelete: true);

        Assert.Equal(17, loaded.Nperiod);
        Assert.Equal(PriceTypePoints.High, loaded.TypePoint);
        Assert.Equal(Color.MediumVioletRed.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexMomentumLegacy";
        using MomentumFileScope scope = new MomentumFileScope(name);

        string legacy = string.Join(
            "\n",
            "11",
            "Low",
            Color.SeaGreen.ToArgb().ToString(),
            "True",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        Momentum loaded = new Momentum(name, canDelete: true);

        Assert.Equal(11, loaded.Nperiod);
        Assert.Equal(PriceTypePoints.Low, loaded.TypePoint);
        Assert.Equal(Color.SeaGreen.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.True(loaded.PaintOn);
    }

    private sealed class MomentumFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public MomentumFileScope(string name)
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
