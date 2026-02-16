using System;
using System.IO;
using System.Linq;
using System.Drawing;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class BollingerPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexBollingerJson";
        using BollingerFileScope scope = new BollingerFileScope(name);

        Bollinger source = new Bollinger(name, canDelete: true)
        {
            ColorUp = Color.LightBlue,
            ColorDown = Color.DarkOrange,
            Length = 25,
            Deviation = 2.4m,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        Bollinger loaded = new Bollinger(name, canDelete: true);

        Assert.Equal(Color.LightBlue.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.DarkOrange.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.Equal(25, loaded.Length);
        Assert.Equal(2.4m, loaded.Deviation);
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexBollingerLegacy";
        using BollingerFileScope scope = new BollingerFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.AliceBlue.ToArgb().ToString(),
            Color.Brown.ToArgb().ToString(),
            "18",
            "3.1",
            "True") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        Bollinger loaded = new Bollinger(name, canDelete: true);

        Assert.Equal(Color.AliceBlue.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.Brown.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.Equal(18, loaded.Length);
        Assert.Equal(3.1m, loaded.Deviation);
        Assert.True(loaded.PaintOn);
    }

    private sealed class BollingerFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public BollingerFileScope(string name)
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
