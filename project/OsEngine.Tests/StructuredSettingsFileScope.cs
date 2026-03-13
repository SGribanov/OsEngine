#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;

namespace OsEngine.Tests;

internal sealed class StructuredSettingsFileScope : IDisposable
{
    private readonly string _settingsDirPath;
    private readonly bool _settingsDirExisted;
    private readonly string[] _trackedPaths;

    public StructuredSettingsFileScope(string canonicalPath)
    {
        CanonicalPath = Path.GetFullPath(canonicalPath);
        LegacyJsonPath = Path.ChangeExtension(CanonicalPath, ".json");
        LegacyTxtPath = Path.ChangeExtension(CanonicalPath, ".txt");

        _settingsDirPath = Path.GetDirectoryName(CanonicalPath)
            ?? throw new InvalidOperationException("Settings directory path is missing.");
        _settingsDirExisted = Directory.Exists(_settingsDirPath);

        if (!_settingsDirExisted)
        {
            Directory.CreateDirectory(_settingsDirPath);
        }

        _trackedPaths = new[]
        {
            CanonicalPath,
            CanonicalPath + ".bak",
            LegacyJsonPath,
            LegacyJsonPath + ".bak",
            LegacyTxtPath,
            LegacyTxtPath + ".bak"
        }
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

        for (int i = 0; i < _trackedPaths.Length; i++)
        {
            string trackedPath = _trackedPaths[i];
            string backupPath = GetBackupPath(trackedPath);

            if (File.Exists(trackedPath))
            {
                File.Copy(trackedPath, backupPath, overwrite: true);
            }
            else if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }

        for (int i = 0; i < _trackedPaths.Length; i++)
        {
            if (File.Exists(_trackedPaths[i]))
            {
                File.Delete(_trackedPaths[i]);
            }
        }
    }

    public string CanonicalPath { get; }

    public string LegacyJsonPath { get; }

    public string LegacyTxtPath { get; }

    public void Dispose()
    {
        for (int i = 0; i < _trackedPaths.Length; i++)
        {
            string trackedPath = _trackedPaths[i];
            string backupPath = GetBackupPath(trackedPath);

            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, trackedPath, overwrite: true);
                File.Delete(backupPath);
            }
            else if (File.Exists(trackedPath))
            {
                File.Delete(trackedPath);
            }
        }

        if (!_settingsDirExisted
            && Directory.Exists(_settingsDirPath)
            && !Directory.EnumerateFileSystemEntries(_settingsDirPath).Any())
        {
            Directory.Delete(_settingsDirPath);
        }
    }

    private static string GetBackupPath(string path)
    {
        return path + ".codex.bak";
    }
}
