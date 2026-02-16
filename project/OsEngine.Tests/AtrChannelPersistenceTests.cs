using System;
using System.IO;
using System.Linq;
using System.Drawing;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class AtrChannelPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexAtrChannelJson";
        using AtrChannelFileScope scope = new AtrChannelFileScope(name);

        AtrChannel source = new AtrChannel(name, canDelete: true)
        {
            ColorBase = Color.CadetBlue,
            Length = 22,
            Multiplier = 2.75m,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        AtrChannel loaded = new AtrChannel(name, canDelete: true);

        Assert.Equal(Color.CadetBlue.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(22, loaded.Length);
        Assert.Equal(2.75m, loaded.Multiplier);
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexAtrChannelLegacy";
        using AtrChannelFileScope scope = new AtrChannelFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.OrangeRed.ToArgb().ToString(),
            "15",
            "3.5",
            "True") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        AtrChannel loaded = new AtrChannel(name, canDelete: true);

        Assert.Equal(Color.OrangeRed.ToArgb(), loaded.ColorBase.ToArgb());
        Assert.Equal(15, loaded.Length);
        Assert.Equal(3.5m, loaded.Multiplier);
        Assert.True(loaded.PaintOn);
    }

    private sealed class AtrChannelFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public AtrChannelFileScope(string name)
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
