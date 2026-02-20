#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class HorizontalVolumePersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        string name = "codex_hv_json_" + Guid.NewGuid().ToString("N");

        using HorizontalVolumeFileScope scope = new HorizontalVolumeFileScope(name);

        HorizontalVolume source = new HorizontalVolume(name);
        source.StepLine = 0.25m;

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        HorizontalVolume loaded = new HorizontalVolume(name);
        Assert.Equal(0.25m, loaded.StepLine);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        string name = "codex_hv_legacy_" + Guid.NewGuid().ToString("N");

        using HorizontalVolumeFileScope scope = new HorizontalVolumeFileScope(name);

        File.WriteAllText(scope.SettingsPath, "0.75");

        HorizontalVolume loaded = new HorizontalVolume(name);
        Assert.Equal(0.75m, loaded.StepLine);
    }

    private sealed class HorizontalVolumeFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public HorizontalVolumeFileScope(string name)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, name + "HorizontalVolumeSet.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _settingsFileExisted = File.Exists(SettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(SettingsPath, _settingsBackup, overwrite: true);
            }
            else if (File.Exists(_settingsBackup))
            {
                File.Delete(_settingsBackup);
            }
        }

        public string SettingsPath { get; }

        public void Dispose()
        {
            if (_settingsFileExisted)
            {
                if (File.Exists(_settingsBackup))
                {
                    File.Copy(_settingsBackup, SettingsPath, overwrite: true);
                    File.Delete(_settingsBackup);
                }
            }
            else
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }

                if (File.Exists(_settingsBackup))
                {
                    File.Delete(_settingsBackup);
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
