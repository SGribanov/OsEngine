#nullable enable

using System;
using System.IO;
using System.Reflection;
using OsEngine.Market.Servers.MFD;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public class MfdTempFileWriteTests
{
    [Fact]
    public void WriteTempFileBytes_ShouldPersistBytes_AndReleaseFileHandle()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-mfd-temp-" + Guid.NewGuid());
        string filePath = Path.Combine(root, "tmpData.txt");
        byte[] payload = [1, 2, 3, 4, 5];

        try
        {
            InvokeWriteTempFileBytes(filePath, payload);

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
    public void WriteTempFileBytes_ShouldReplaceExistingFileWithoutLeavingTempFile()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-mfd-temp-" + Guid.NewGuid());
        string filePath = Path.Combine(root, "tmpData.txt");
        byte[] oldPayload = [9, 9, 9];
        byte[] newPayload = [7, 8, 9, 10];

        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllBytes(filePath, oldPayload);

            InvokeWriteTempFileBytes(filePath, newPayload);

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

    private static void InvokeWriteTempFileBytes(string filePath, byte[] payload)
    {
        MethodInfo method = typeof(MfdServerRealization).GetMethod(
                                "WriteTempFileBytes",
                                BindingFlags.Static | BindingFlags.NonPublic)
                            ?? throw new InvalidOperationException("Method not found: WriteTempFileBytes");

        method.Invoke(null, [filePath, payload]);
    }
}
