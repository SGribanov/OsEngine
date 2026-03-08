#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Reflection;
using System.Text;
using OsEngine.Market.Servers.MoexFixFastTwimeFutures;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public class MoexFixFastTwimeFuturesLogWriterTests
{
    [Fact]
    public void CreatePersistentLogWriter_ShouldCreateDirectory_TruncateExistingFile_AndKeepCurrentLineShapeWithoutBom()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-moextwime-log-" + Guid.NewGuid());
        string path = Path.Combine(root, "nested", "trades.log");
        MethodInfo createMethod = GetPrivateMethod("CreatePersistentLogWriter");
        MethodInfo formatMethod = GetPrivateMethod("FormatPersistentLogLine");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "stale-line" + Environment.NewLine, Encoding.UTF8);

            string line = (string)formatMethod.Invoke(null, new object[] { "trade payload", new DateTime(2026, 3, 8, 9, 15, 30) })!;

            using (StreamWriter writer = (StreamWriter)createMethod.Invoke(null, new object[] { path, false })!)
            {
                writer.WriteLine(line);
            }

            byte[] bytes = File.ReadAllBytes(path);
            string[] lines = File.ReadAllLines(path);

            Assert.True(Directory.Exists(Path.GetDirectoryName(path)!));
            Assert.Equal(new[] { line }, lines);
            Assert.False(bytes.Length >= 3
                         && bytes[0] == 0xEF
                         && bytes[1] == 0xBB
                         && bytes[2] == 0xBF);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void CreatePersistentLogWriter_ShouldAppendWhenRequested()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-moextwime-log-" + Guid.NewGuid());
        string path = Path.Combine(root, "TradingServerLog.log");
        MethodInfo createMethod = GetPrivateMethod("CreatePersistentLogWriter");
        MethodInfo formatMethod = GetPrivateMethod("FormatPersistentLogLine");

        try
        {
            Directory.CreateDirectory(root);

            string firstLine = (string)formatMethod.Invoke(null, new object[] { "first-event", new DateTime(2026, 3, 8, 9, 15, 30) })!;
            string secondLine = (string)formatMethod.Invoke(null, new object[] { "second-event", new DateTime(2026, 3, 8, 9, 15, 31) })!;

            File.WriteAllText(path, firstLine + Environment.NewLine, new UTF8Encoding(false));

            using (StreamWriter writer = (StreamWriter)createMethod.Invoke(null, new object[] { path, true })!)
            {
                writer.WriteLine(secondLine);
            }

            Assert.Equal(new[] { firstLine, secondLine }, File.ReadAllLines(path));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private static MethodInfo GetPrivateMethod(string methodName)
    {
        return typeof(MoexFixFastTwimeFuturesServerRealization).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
               ?? throw new InvalidOperationException("Method not found: " + methodName);
    }
}
