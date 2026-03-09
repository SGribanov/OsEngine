#nullable enable

using System;
using System.IO;
using System.Reflection;
using OsEngine.Market.Servers.QscalpMarketDepth;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public class QscalpMarketDepthTempQshWriteTests
{
    [Fact]
    public void WriteTempQshBytes_ShouldPersistBytes_AndReleaseFileHandle()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-qscalp-qsh-" + Guid.NewGuid());
        string filePath = Path.Combine(root, "depth.qsh");
        byte[] payload = [1, 2, 3, 4, 5];

        try
        {
            InvokeWriteTempQshBytes(filePath, payload);

            Assert.Equal(payload, File.ReadAllBytes(filePath));
            Assert.False(File.Exists(filePath + ".tmp"));

            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.Equal(payload.Length, stream.Length);
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
    public void WriteTempQshBytes_ShouldReplaceExistingFileWithoutLeavingTempFile()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-qscalp-qsh-" + Guid.NewGuid());
        string filePath = Path.Combine(root, "depth.qsh");
        byte[] oldPayload = [9, 9, 9];
        byte[] newPayload = [7, 8, 9, 10];

        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllBytes(filePath, oldPayload);

            InvokeWriteTempQshBytes(filePath, newPayload);

            Assert.Equal(newPayload, File.ReadAllBytes(filePath));
            Assert.False(File.Exists(filePath + ".tmp"));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private static void InvokeWriteTempQshBytes(string filePath, byte[] payload)
    {
        MethodInfo method = typeof(QscalpMarketDepthServerRealization).GetMethod(
                                "WriteTempQshBytes",
                                BindingFlags.Static | BindingFlags.NonPublic)
                            ?? throw new InvalidOperationException("Method not found: WriteTempQshBytes");

        method.Invoke(null, [filePath, payload]);
    }
}
