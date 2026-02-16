using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Market.Servers.Tester;
using Xunit;

namespace OsEngine.Tests;

public class TesterServerSecurityTestSettingsPersistenceTests
{
    [Fact]
    public void SaveSecurityTestSettings_ShouldPersistJson_AndLoadRoundTrip()
    {
        using TesterSecuritySettingsFileScope scope = new TesterSecuritySettingsFileScope();

        TesterServer source = scope.CreateWithoutConstructor();
        scope.ConfigureFolderMode(source);
        source.TimeStart = new DateTime(2025, 1, 2, 3, 4, 5);
        source.TimeEnd = new DateTime(2025, 1, 3, 4, 5, 6);
        source.SaveSecurityTestSettings();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        TesterServer loaded = scope.CreateWithoutConstructor();
        scope.ConfigureFolderMode(loaded);
        loaded.LoadSecurityTestSettings();

        Assert.Equal(new DateTime(2025, 1, 2, 3, 4, 5), loaded.TimeStart);
        Assert.Equal(new DateTime(2025, 1, 3, 4, 5, 6), loaded.TimeEnd);
    }

    [Fact]
    public void LoadSecurityTestSettings_ShouldSupportLegacyLineBasedFormat()
    {
        using TesterSecuritySettingsFileScope scope = new TesterSecuritySettingsFileScope();

        DateTime start = new DateTime(2024, 5, 6, 7, 8, 9);
        DateTime end = new DateTime(2024, 5, 7, 8, 9, 10);

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            start.ToString(CultureInfo.InvariantCulture),
            end.ToString(CultureInfo.InvariantCulture)
        });

        TesterServer loaded = scope.CreateWithoutConstructor();
        scope.ConfigureFolderMode(loaded);
        loaded.LoadSecurityTestSettings();

        Assert.Equal(start, loaded.TimeStart);
        Assert.Equal(end, loaded.TimeEnd);
    }

    private sealed class TesterSecuritySettingsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _folderExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly FieldInfo _sourceDataTypeField;
        private readonly FieldInfo _pathToFolderField;

        public TesterSecuritySettingsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            FolderPath = Path.Combine(_engineDirPath, "TestSecuritySettings");
            SettingsPath = Path.Combine(FolderPath, "SecurityTestSettings.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _sourceDataTypeField = typeof(TesterServer).GetField("_sourceDataType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _sourceDataType not found.");
            _pathToFolderField = typeof(TesterServer).GetField("_pathToFolder", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _pathToFolder not found.");

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _folderExisted = Directory.Exists(FolderPath);
            if (!_folderExisted)
            {
                Directory.CreateDirectory(FolderPath);
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

        public string FolderPath { get; }

        public string SettingsPath { get; }

        public TesterServer CreateWithoutConstructor()
        {
            return (TesterServer)RuntimeHelpers.GetUninitializedObject(typeof(TesterServer));
        }

        public void ConfigureFolderMode(TesterServer server)
        {
            _sourceDataTypeField.SetValue(server, TesterSourceDataType.Folder);
            _pathToFolderField.SetValue(server, FolderPath);
        }

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

            if (!_folderExisted
                && Directory.Exists(FolderPath)
                && !Directory.EnumerateFileSystemEntries(FolderPath).Any())
            {
                Directory.Delete(FolderPath);
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
