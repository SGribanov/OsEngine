#nullable enable

using System;
using System.IO;
using System.Reflection;
using System.Text;
using OsEngine.Market.Servers.GateIoData;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public class GateIoDataExtractedCsvWriteTests
{
    [Fact]
    public void PersistExtractedCsv_ShouldPersistContent_AndReleaseFileHandle()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-gateio-extract-" + Guid.NewGuid());
        string csvPath = Path.Combine(root, "monthly.csv");

        try
        {
            using MemoryStream content = new MemoryStream(Encoding.UTF8.GetBytes("1,2,3\n4,5,6\n"));

            InvokePersistExtractedCsv(csvPath, content);

            Assert.Equal("1,2,3\n4,5,6\n", File.ReadAllText(csvPath));
            Assert.False(File.Exists(csvPath + ".tmp"));

            using FileStream stream = new FileStream(csvPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.True(stream.Length > 0);
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
    public void PersistExtractedCsv_ShouldReplaceExistingFile_WithoutLeavingTempFile()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-gateio-extract-" + Guid.NewGuid());
        string csvPath = Path.Combine(root, "monthly.csv");

        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(csvPath, "old");
            using MemoryStream content = new MemoryStream(Encoding.UTF8.GetBytes("new"));

            InvokePersistExtractedCsv(csvPath, content);

            Assert.Equal("new", File.ReadAllText(csvPath));
            Assert.False(File.Exists(csvPath + ".tmp"));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private static void InvokePersistExtractedCsv(string csvPath, Stream content)
    {
        MethodInfo method = typeof(GateIoDataServerRealization).GetMethod(
                                "PersistExtractedCsv",
                                BindingFlags.Static | BindingFlags.NonPublic)
                            ?? throw new InvalidOperationException("Method not found: PersistExtractedCsv");

        method.Invoke(null, [csvPath, content]);
    }
}
