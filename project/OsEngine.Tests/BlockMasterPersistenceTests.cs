using System;
using System.IO;
using System.Linq;
using OsEngine.OsTrader.Gui.BlockInterface;
using Xunit;

namespace OsEngine.Tests;

public class BlockMasterPersistenceTests
{
    [Fact]
    public void Password_ShouldPersistJson_AndLoadRoundTrip()
    {
        using BlockMasterFileScope scope = new BlockMasterFileScope();

        BlockMaster.Password = "my-secret";

        string content = File.ReadAllText(scope.PasswordPath);
        Assert.StartsWith("{", content.TrimStart());
        Assert.Equal("my-secret", BlockMaster.Password);
    }

    [Fact]
    public void Password_ShouldSupportLegacyLineBasedFormat()
    {
        using BlockMasterFileScope scope = new BlockMasterFileScope();

        File.WriteAllLines(scope.PasswordPath, new[] { BlockMaster.Encrypt("legacy-secret") });

        Assert.Equal("legacy-secret", BlockMaster.Password);
    }

    [Fact]
    public void IsBlocked_ShouldPersistJson_AndLoadRoundTrip()
    {
        using BlockMasterFileScope scope = new BlockMasterFileScope();

        BlockMaster.IsBlocked = true;

        string content = File.ReadAllText(scope.IsBlockedPath);
        Assert.StartsWith("{", content.TrimStart());
        Assert.True(BlockMaster.IsBlocked);
    }

    [Fact]
    public void IsBlocked_ShouldSupportLegacyLineBasedFormat()
    {
        using BlockMasterFileScope scope = new BlockMasterFileScope();

        File.WriteAllLines(scope.IsBlockedPath, new[] { BlockMaster.Encrypt("True") });

        Assert.True(BlockMaster.IsBlocked);
    }

    private sealed class BlockMasterFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _passwordFileExisted;
        private readonly bool _isBlockedFileExisted;
        private readonly string _passwordBackup;
        private readonly string _isBlockedBackup;

        public BlockMasterFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            PasswordPath = Path.Combine(_engineDirPath, "PrimeSettingss.txt");
            IsBlockedPath = Path.Combine(_engineDirPath, "PrimeSettingsss.txt");
            _passwordBackup = PasswordPath + ".codex.bak";
            _isBlockedBackup = IsBlockedPath + ".codex.bak";

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _passwordFileExisted = File.Exists(PasswordPath);
            if (_passwordFileExisted)
            {
                File.Copy(PasswordPath, _passwordBackup, overwrite: true);
            }
            else if (File.Exists(_passwordBackup))
            {
                File.Delete(_passwordBackup);
            }

            _isBlockedFileExisted = File.Exists(IsBlockedPath);
            if (_isBlockedFileExisted)
            {
                File.Copy(IsBlockedPath, _isBlockedBackup, overwrite: true);
            }
            else if (File.Exists(_isBlockedBackup))
            {
                File.Delete(_isBlockedBackup);
            }
        }

        public string PasswordPath { get; }

        public string IsBlockedPath { get; }

        public void Dispose()
        {
            RestoreFile(PasswordPath, _passwordBackup, _passwordFileExisted);
            RestoreFile(IsBlockedPath, _isBlockedBackup, _isBlockedFileExisted);

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }

        private static void RestoreFile(string path, string backupPath, bool existed)
        {
            if (existed)
            {
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, path, overwrite: true);
                    File.Delete(backupPath);
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
        }
    }
}
