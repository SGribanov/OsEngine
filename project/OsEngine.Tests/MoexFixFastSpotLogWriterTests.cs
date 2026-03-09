#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Reflection;
using System.Text;
using OsEngine.Market.Servers.MoexFixFastSpot;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public class MoexFixFastSpotLogWriterTests
{
    [Fact]
    public void CreatePersistentLogWriter_ShouldCreateDirectory_TruncateUdpLog_AndUseUtf8WithoutBom()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-moexspot-log-" + Guid.NewGuid());
        string path = Path.Combine(root, "nested", "FIXFAST_Multicast_UDP-log.txt");
        MethodInfo createMethod = GetPrivateMethod("CreatePersistentLogWriter");
        MethodInfo formatUdpMethod = GetPrivateMethod("FormatUdpLogLine");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "stale-line" + Environment.NewLine, Encoding.UTF8);

            string line = (string)formatUdpMethod.Invoke(null, new object[] { "InstrumentDefinitionsReader", "trade payload", new DateTime(2026, 3, 8, 9, 15, 30) })!;

            using (StreamWriter writer = (StreamWriter)createMethod.Invoke(null, new object[] { path, false })!)
            {
                writer.WriteLine(line);
            }

            byte[] bytes = File.ReadAllBytes(path);

            Assert.True(Directory.Exists(Path.GetDirectoryName(path)!));
            Assert.Equal(new[] { line }, File.ReadAllLines(path));
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
    public void CreatePersistentLogWriter_ShouldAppendToExistingMfixLog()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-moexspot-log-" + Guid.NewGuid());
        string path = Path.Combine(root, "MFIX_2026-03-08.txt");
        MethodInfo createMethod = GetPrivateMethod("CreatePersistentLogWriter");
        MethodInfo formatMfixMethod = GetPrivateMethod("FormatMfixLogLine");

        try
        {
            Directory.CreateDirectory(root);

            string firstLine = (string)formatMfixMethod.Invoke(null, new object[] { ">>>", "first-fix", new DateTime(2026, 3, 8, 9, 15, 30, DateTimeKind.Utc) })!;
            string secondLine = (string)formatMfixMethod.Invoke(null, new object[] { "<<<", "second-fix", new DateTime(2026, 3, 8, 9, 15, 31, DateTimeKind.Utc) })!;

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

    [Fact]
    public void WritePersistentLogLine_ShouldRecreateDisposedWriter_AndAppendMfixLine()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-moexspot-log-" + Guid.NewGuid());
        string path = Path.Combine(root, "MFIX_2026-03-08.txt");
        MethodInfo createMethod = GetPrivateMethod("CreatePersistentLogWriter");
        MethodInfo writeMethod = GetPrivateMethod("WritePersistentLogLine");
        MethodInfo formatMfixMethod = GetPrivateMethod("FormatMfixLogLine");

        try
        {
            Directory.CreateDirectory(root);

            string firstLine = (string)formatMfixMethod.Invoke(null, new object[] { ">>>", "first-fix", new DateTime(2026, 3, 8, 9, 15, 30, DateTimeKind.Utc) })!;
            string secondLine = (string)formatMfixMethod.Invoke(null, new object[] { "<<<", "second-fix", new DateTime(2026, 3, 8, 9, 15, 31, DateTimeKind.Utc) })!;

            StreamWriter disposedWriter = (StreamWriter)createMethod.Invoke(null, new object[] { path, true })!;
            disposedWriter.WriteLine(firstLine);
            disposedWriter.Dispose();

            object?[] args = { disposedWriter, path, true, secondLine };
            writeMethod.Invoke(null, args);

            StreamWriter recreatedWriter = Assert.IsType<StreamWriter>(args[0]);
            recreatedWriter.Dispose();

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

    [Fact]
    public void FormatLogLines_ShouldPreserveCurrentUdpAndMfixShapes()
    {
        MethodInfo formatUdpMethod = GetPrivateMethod("FormatUdpLogLine");
        MethodInfo formatMfixMethod = GetPrivateMethod("FormatMfixLogLine");
        DateTime udpTimestamp = new DateTime(2026, 3, 8, 9, 15, 30);
        DateTime mfixTimestamp = new DateTime(2026, 3, 8, 9, 15, 31, DateTimeKind.Utc);

        string udpLine = (string)formatUdpMethod.Invoke(null, new object[] { "OrderSnapshotsReader", "msg body", udpTimestamp })!;
        string mfixLine = (string)formatMfixMethod.Invoke(null, new object[] { ">>>", "35=A|49=CLIENT", mfixTimestamp })!;

        Assert.Equal($"{udpTimestamp} OrderSnapshotsReader: msg body", udpLine);
        Assert.Equal($"{mfixTimestamp} >>> [MFIX Trade]: 35=A|49=CLIENT", mfixLine);
    }

    private static MethodInfo GetPrivateMethod(string methodName)
    {
        return typeof(MoexFixFastSpotServerRealization).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
               ?? throw new InvalidOperationException("Method not found: " + methodName);
    }
}
