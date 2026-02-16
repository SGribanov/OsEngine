using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class VerticalHorizontalFilterPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexVhfJson";
        using VerticalHorizontalFilterFileScope scope = new VerticalHorizontalFilterFileScope(name);

        VerticalHorizontalFilter source = new VerticalHorizontalFilter(name, canDelete: true)
        {
            Nperiod = 31,
            ColorBase = Color.MidnightBlue,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        VerticalHorizontalFilter loaded = new VerticalHorizontalFilter(name, canDelete: true);

        Assert.Equal(31, loaded.Nperiod);
        Assert.Equal(Color.MidnightBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexVhfLegacy";
        using VerticalHorizontalFilterFileScope scope = new VerticalHorizontalFilterFileScope(name);

        string legacy = string.Join(
            "\n",
            "28",
            Color.DarkBlue.ToArgb().ToString(),
            "True",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        VerticalHorizontalFilter loaded = new VerticalHorizontalFilter(name, canDelete: true);

        Assert.Equal(28, loaded.Nperiod);
        Assert.Equal(Color.DarkBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.True(loaded.PaintOn);
    }

    private sealed class VerticalHorizontalFilterFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public VerticalHorizontalFilterFileScope(string name)
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
