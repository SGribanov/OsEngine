#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class AdaptiveLookBackPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexAdaptiveLookBackJson";
        using AdaptiveLookBackFileScope scope = new AdaptiveLookBackFileScope(name);

        AdaptiveLookBack source = new AdaptiveLookBack(name, canDelete: true)
        {
            ColorBase = Color.MediumBlue,
            Length = 13,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        AdaptiveLookBack loaded = new AdaptiveLookBack(name, canDelete: true);

        Assert.Equal(Color.MediumBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(13, loaded.Length);
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexAdaptiveLookBackLegacy";
        using AdaptiveLookBackFileScope scope = new AdaptiveLookBackFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.DarkSlateGray.ToArgb().ToString(),
            "8",
            "True",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        AdaptiveLookBack loaded = new AdaptiveLookBack(name, canDelete: true);

        Assert.Equal(Color.DarkSlateGray.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(8, loaded.Length);
        Assert.True(loaded.PaintOn);
    }

    private sealed class AdaptiveLookBackFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public AdaptiveLookBackFileScope(string name)
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
