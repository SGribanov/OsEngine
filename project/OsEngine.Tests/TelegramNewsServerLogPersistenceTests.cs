#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Market.Servers.TelegramNews;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public class TelegramNewsServerLogPersistenceTests
{
    [Fact]
    public void AppendTelegramLogLine_ShouldPreserveAppendOrder_AndCurrentLineFormat()
    {
        using TelegramNewsLogFileScope scope = new TelegramNewsLogFileScope();
        MethodInfo appendMethod = GetPrivateMethod("AppendTelegramLogLine");

        appendMethod.Invoke(null, new object[] { scope.LogPath, "first message", new DateTime(2026, 3, 8, 9, 15, 30) });
        appendMethod.Invoke(null, new object[] { scope.LogPath, "second message", new DateTime(2026, 3, 8, 9, 15, 31) });

        string expected =
            "[09:15:30] LogEvent : first message\n" +
            "[09:15:31] LogEvent : second message\n";

        Assert.Equal(expected, File.ReadAllText(scope.LogPath));
    }

    [Fact]
    public void EnsureTelegramLogIsWithinSizeLimit_ShouldTruncateOnlyWhenFileExceedsLimit()
    {
        using TelegramNewsLogFileScope scope = new TelegramNewsLogFileScope();
        MethodInfo ensureMethod = GetPrivateMethod("EnsureTelegramLogIsWithinSizeLimit");

        File.WriteAllText(scope.LogPath, new string('a', 1024 * 1024));
        ensureMethod.Invoke(null, new object[] { scope.LogPath });
        Assert.Equal(1024 * 1024, new FileInfo(scope.LogPath).Length);

        File.WriteAllText(scope.LogPath, new string('b', 1024 * 1024 + 1));
        ensureMethod.Invoke(null, new object[] { scope.LogPath });
        Assert.Equal(string.Empty, File.ReadAllText(scope.LogPath));
    }

    private static MethodInfo GetPrivateMethod(string name)
    {
        return typeof(TelegramNewsServerRealization).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
               ?? throw new InvalidOperationException("Method not found: " + name);
    }

    private sealed class TelegramNewsLogFileScope : IDisposable
    {
        private readonly string _logDirPath;
        private readonly bool _logDirExisted;
        private readonly bool _logFileExisted;
        private readonly string _logFileBackup;

        public TelegramNewsLogFileScope()
        {
            _logDirPath = Path.GetFullPath(Path.Combine("Engine", "Log", "TelegramLogs"));
            LogPath = Path.Combine(_logDirPath, "wteleg.log");
            _logFileBackup = LogPath + ".codex.bak";

            _logDirExisted = Directory.Exists(_logDirPath);
            if (!_logDirExisted)
            {
                Directory.CreateDirectory(_logDirPath);
            }

            _logFileExisted = File.Exists(LogPath);
            if (_logFileExisted)
            {
                File.Copy(LogPath, _logFileBackup, overwrite: true);
            }
            else if (File.Exists(_logFileBackup))
            {
                File.Delete(_logFileBackup);
            }
        }

        public string LogPath { get; }

        public void Dispose()
        {
            if (_logFileExisted)
            {
                if (File.Exists(_logFileBackup))
                {
                    File.Copy(_logFileBackup, LogPath, overwrite: true);
                    File.Delete(_logFileBackup);
                }
            }
            else
            {
                if (File.Exists(LogPath))
                {
                    File.Delete(LogPath);
                }

                if (File.Exists(_logFileBackup))
                {
                    File.Delete(_logFileBackup);
                }
            }

            if (!_logDirExisted
                && Directory.Exists(_logDirPath)
                && !Directory.EnumerateFileSystemEntries(_logDirPath).Any())
            {
                Directory.Delete(_logDirPath);
            }
        }
    }
}
