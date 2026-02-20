#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class DonchianChannelPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexDonchianJson";
        using DonchianChannelFileScope scope = new DonchianChannelFileScope(name);

        DonchianChannel source = new DonchianChannel(name, canDelete: true)
        {
            ColorUp = Color.DarkBlue,
            ColorAvg = Color.DarkGray,
            ColorDown = Color.DarkGreen,
            Length = 17,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        DonchianChannel loaded = new DonchianChannel(name, canDelete: true);

        Assert.Equal(Color.DarkBlue.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.DarkGray.ToArgb(), loaded.ColorAvg.ToArgb());
        Assert.Equal(Color.DarkGreen.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.Equal(17, loaded.Length);
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexDonchianLegacy";
        using DonchianChannelFileScope scope = new DonchianChannelFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.CadetBlue.ToArgb().ToString(),
            Color.Crimson.ToArgb().ToString(),
            Color.DarkOliveGreen.ToArgb().ToString(),
            "29",
            "True") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        DonchianChannel loaded = new DonchianChannel(name, canDelete: true);

        Assert.Equal(Color.CadetBlue.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.Crimson.ToArgb(), loaded.ColorAvg.ToArgb());
        Assert.Equal(Color.DarkOliveGreen.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.Equal(29, loaded.Length);
        Assert.True(loaded.PaintOn);
    }

    private sealed class DonchianChannelFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public DonchianChannelFileScope(string name)
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
