#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using OsEngine.UpdateModule;
using Xunit;

namespace OsEngine.Tests;

public class MainWindowUpdaterPersistenceTests
{
    [Fact]
    public void SaveLastUpdatesInfo_ShouldPersistTimestamp_AndReplaceExistingFileAtomically()
    {
        using UpdaterMetadataFileScope scope = new UpdaterMetadataFileScope();
        MethodInfo saveMethod = typeof(OsEngine.MainWindow).GetMethod("SaveLastUpdatesInfo", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method SaveLastUpdatesInfo not found.");

        DateTime firstTimestamp = new DateTime(2026, 3, 7, 9, 15, 30);
        DateTime secondTimestamp = new DateTime(2026, 3, 8, 11, 45, 10);

        saveMethod.Invoke(null, new object[] { firstTimestamp });
        Assert.Equal(firstTimestamp.ToString("G"), File.ReadAllText(scope.LastUpdatesInfoPath));

        saveMethod.Invoke(null, new object[] { secondTimestamp });

        Assert.Equal(secondTimestamp.ToString("G"), File.ReadAllText(scope.LastUpdatesInfoPath));
        Assert.True(File.Exists(scope.LastUpdatesInfoBackupPath));
        Assert.Equal(firstTimestamp.ToString("G"), File.ReadAllText(scope.LastUpdatesInfoBackupPath));
    }

    [Fact]
    public void WriteFilesVersionsTime_ShouldPersistExpectedFormat_AndRespectCutoffTime()
    {
        using UpdaterMetadataFileScope scope = new UpdaterMetadataFileScope();
        OsEngine.MainWindow mainWindow = CreateMainWindowWithoutConstructor();
        FieldInfo responseField = typeof(OsEngine.MainWindow).GetField("_updServerResp", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field _updServerResp not found.");
        MethodInfo writeMethod = typeof(OsEngine.MainWindow).GetMethod("WriteFilesVersionsTime", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method WriteFilesVersionsTime not found.");

        DateTime buildTime = new DateTime(2026, 3, 7, 9, 15, 30);
        DateTime earlierServerTime = new DateTime(2026, 3, 6, 18, 5, 0);
        DateTime laterServerTime = new DateTime(2026, 3, 9, 7, 0, 0);

        responseField.SetValue(mainWindow, new UpdateResponse
        {
            Files = new List<GithubFileInfo>
            {
                new GithubFileInfo { Name = "first.dll", LastUpdate = earlierServerTime, Size = 10 },
                new GithubFileInfo { Name = "second.dll", LastUpdate = laterServerTime, Size = 22 }
            }
        });

        writeMethod.Invoke(mainWindow, new object[] { buildTime });

        StringBuilder expected = new StringBuilder();
        expected.AppendLine("first.dll#" + earlierServerTime + "#10");
        expected.AppendLine("second.dll#" + buildTime + "#22");

        Assert.Equal(expected.ToString(), File.ReadAllText(scope.FilesVersionsTimePath));
    }

    private static OsEngine.MainWindow CreateMainWindowWithoutConstructor()
    {
        return (OsEngine.MainWindow)RuntimeHelpers.GetUninitializedObject(typeof(OsEngine.MainWindow));
    }

    private sealed class UpdaterMetadataFileScope : IDisposable
    {
        private readonly string _updaterDirPath;
        private readonly bool _updaterDirExisted;
        private readonly List<FileRestoreState> _restoreStates;

        public UpdaterMetadataFileScope()
        {
            _updaterDirPath = Path.GetFullPath(Path.Combine("Engine", "Updater"));
            _updaterDirExisted = Directory.Exists(_updaterDirPath);

            if (!_updaterDirExisted)
            {
                Directory.CreateDirectory(_updaterDirPath);
            }

            LastUpdatesInfoPath = Path.Combine(_updaterDirPath, "LastUpdatesInfo.txt");
            FilesVersionsTimePath = Path.Combine(_updaterDirPath, "FilesVersionsTime.txt");
            LastUpdatesInfoBackupPath = LastUpdatesInfoPath + ".bak";
            FilesVersionsTimeBackupPath = FilesVersionsTimePath + ".bak";

            _restoreStates = new List<FileRestoreState>
            {
                new FileRestoreState(LastUpdatesInfoPath),
                new FileRestoreState(FilesVersionsTimePath),
                new FileRestoreState(LastUpdatesInfoBackupPath),
                new FileRestoreState(FilesVersionsTimeBackupPath)
            };
        }

        public string LastUpdatesInfoPath { get; }

        public string FilesVersionsTimePath { get; }

        public string LastUpdatesInfoBackupPath { get; }

        public string FilesVersionsTimeBackupPath { get; }

        public void Dispose()
        {
            for (int i = _restoreStates.Count - 1; i >= 0; i--)
            {
                _restoreStates[i].Restore();
            }

            if (!_updaterDirExisted
                && Directory.Exists(_updaterDirPath)
                && !Directory.EnumerateFileSystemEntries(_updaterDirPath).Any())
            {
                Directory.Delete(_updaterDirPath);
            }
        }

        private sealed class FileRestoreState
        {
            private readonly string _path;
            private readonly bool _existed;
            private readonly string _backupPath;

            public FileRestoreState(string path)
            {
                _path = path;
                _existed = File.Exists(path);
                _backupPath = path + ".codex-test-backup";

                if (_existed)
                {
                    File.Copy(path, _backupPath, overwrite: true);
                }
                else if (File.Exists(_backupPath))
                {
                    File.Delete(_backupPath);
                }
            }

            public void Restore()
            {
                if (_existed)
                {
                    if (File.Exists(_backupPath))
                    {
                        File.Copy(_backupPath, _path, overwrite: true);
                        File.Delete(_backupPath);
                    }
                }
                else
                {
                    if (File.Exists(_path))
                    {
                        File.Delete(_path);
                    }

                    if (File.Exists(_backupPath))
                    {
                        File.Delete(_backupPath);
                    }
                }
            }
        }
    }
}
