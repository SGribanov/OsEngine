#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class FractalPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexFractalJson";
        using FractalFileScope scope = new FractalFileScope(name);

        Fractal source = new Fractal(name, canDelete: true)
        {
            PaintOn = false,
            ColorUp = Color.DarkBlue,
            ColorDown = Color.DarkRed
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        Fractal loaded = new Fractal(name, canDelete: true);

        Assert.False(loaded.PaintOn);
        Assert.Equal(Color.DarkBlue.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.DarkRed.ToArgb(), loaded.ColorDown.ToArgb());
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexFractalLegacy";
        using FractalFileScope scope = new FractalFileScope(name);

        string legacy = string.Join(
            "\n",
            "True",
            Color.Cyan.ToArgb().ToString(),
            Color.Maroon.ToArgb().ToString()) + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        Fractal loaded = new Fractal(name, canDelete: true);

        Assert.True(loaded.PaintOn);
        Assert.Equal(Color.Cyan.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.Maroon.ToArgb(), loaded.ColorDown.ToArgb());
    }

    private sealed class FractalFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public FractalFileScope(string name)
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
