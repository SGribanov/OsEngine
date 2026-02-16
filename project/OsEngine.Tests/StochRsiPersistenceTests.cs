using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class StochRsiPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexStochRsiJson";
        using StochRsiFileScope scope = new StochRsiFileScope(name);

        StochRsi source = new StochRsi(name, canDelete: true)
        {
            ColorK = Color.DeepSkyBlue,
            RsiLength = 17,
            StochasticLength = 12,
            K = 4,
            D = 5
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        StochRsi loaded = new StochRsi(name, canDelete: true);

        Assert.Equal(Color.DeepSkyBlue.ToArgb(), loaded.ColorK.ToArgb());
        Assert.Equal(17, loaded.RsiLength);
        Assert.Equal(12, loaded.StochasticLength);
        Assert.Equal(4, loaded.K);
        Assert.Equal(5, loaded.D);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexStochRsiLegacy";
        using StochRsiFileScope scope = new StochRsiFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.LimeGreen.ToArgb().ToString(),
            "21",
            "9",
            "3",
            "4") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        StochRsi loaded = new StochRsi(name, canDelete: true);

        Assert.Equal(Color.LimeGreen.ToArgb(), loaded.ColorK.ToArgb());
        Assert.Equal(21, loaded.RsiLength);
        Assert.Equal(9, loaded.StochasticLength);
        Assert.Equal(3, loaded.K);
        Assert.Equal(4, loaded.D);
    }

    private sealed class StochRsiFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public StochRsiFileScope(string name)
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
