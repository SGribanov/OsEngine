#nullable enable

using System;
using System.IO;
using System.Reflection;
using OsEngine.Market.Servers.BinanceData;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public class BinanceDataArchiveWriteTests
{
    [Fact]
    public void WriteTempArchiveBytes_ShouldPersistBytes_AndReleaseFileHandle()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-binance-archive-" + Guid.NewGuid());
        string archivePath = Path.Combine(root, "archive.zip");
        byte[] payload = [1, 2, 3, 4, 5];

        try
        {
            InvokeWriteTempArchiveBytes(archivePath, payload);

            Assert.Equal(payload, File.ReadAllBytes(archivePath));
            Assert.False(File.Exists(archivePath + ".tmp"));

            using FileStream stream = new FileStream(archivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
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
    public void WriteTempArchiveBytes_ShouldReplaceExistingArchiveWithoutLeavingTempFile()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-binance-archive-" + Guid.NewGuid());
        string archivePath = Path.Combine(root, "archive.zip");
        byte[] oldPayload = [9, 9, 9];
        byte[] newPayload = [7, 8, 9, 10];

        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllBytes(archivePath, oldPayload);

            InvokeWriteTempArchiveBytes(archivePath, newPayload);

            Assert.Equal(newPayload, File.ReadAllBytes(archivePath));
            Assert.False(File.Exists(archivePath + ".tmp"));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private static void InvokeWriteTempArchiveBytes(string archivePath, byte[] payload)
    {
        MethodInfo method = typeof(BinanceDataServerRealization).GetMethod(
                                "WriteTempArchiveBytes",
                                BindingFlags.Static | BindingFlags.NonPublic)
                            ?? throw new InvalidOperationException("Method not found: WriteTempArchiveBytes");

        method.Invoke(null, [archivePath, payload]);
    }
}
