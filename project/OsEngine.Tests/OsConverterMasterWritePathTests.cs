#nullable enable

using System;
using System.IO;
using System.Text;
using OsEngine.OsConverter;

namespace OsEngine.Tests;

public class OsConverterMasterWritePathTests
{
    [Fact]
    public void CreateStreamingTempOutputPath_ShouldStayInDestinationDirectory()
    {
        using TempDirectoryScope scope = new TempDirectoryScope();
        string exitPath = Path.Combine(scope.DirectoryPath, "candles.txt");

        string tempPath = OsConverterMaster.CreateStreamingTempOutputPath(exitPath);

        Assert.Equal(scope.DirectoryPath, Path.GetDirectoryName(tempPath));
        Assert.EndsWith(".tmp", tempPath, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(Path.GetFullPath(exitPath), tempPath);
    }

    [Fact]
    public void CreateStreamingTempWriter_ShouldWriteUtf8WithoutBom()
    {
        using TempDirectoryScope scope = new TempDirectoryScope();
        string tempPath = Path.Combine(scope.DirectoryPath, "candles.tmp");
        const string expectedContent = "price=\u20AC";

        using (StreamWriter writer = OsConverterMaster.CreateStreamingTempWriter(tempPath))
        {
            writer.Write(expectedContent);
            OsConverterMaster.FlushStreamingTempWriter(writer);
        }

        byte[] bytes = File.ReadAllBytes(tempPath);

        Assert.False(HasUtf8Bom(bytes));
        Assert.Equal(expectedContent, Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public void PromoteStreamingOutput_ShouldReplaceExistingDestinationWithoutBackup()
    {
        using TempDirectoryScope scope = new TempDirectoryScope();
        string exitPath = Path.Combine(scope.DirectoryPath, "candles.txt");
        string tempPath = Path.Combine(scope.DirectoryPath, "candles.tmp");

        File.WriteAllText(exitPath, "old", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        File.WriteAllText(tempPath, "new", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        OsConverterMaster.PromoteStreamingOutput(tempPath, exitPath);

        Assert.Equal("new", File.ReadAllText(exitPath));
        Assert.False(File.Exists(tempPath));
        Assert.False(File.Exists(exitPath + ".bak"));
    }

    [Fact]
    public void PromoteStreamingOutput_ShouldMoveTempWhenDestinationDoesNotExist()
    {
        using TempDirectoryScope scope = new TempDirectoryScope();
        string exitPath = Path.Combine(scope.DirectoryPath, "candles.txt");
        string tempPath = Path.Combine(scope.DirectoryPath, "candles.tmp");

        File.WriteAllText(tempPath, "new", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        OsConverterMaster.PromoteStreamingOutput(tempPath, exitPath);

        Assert.Equal("new", File.ReadAllText(exitPath));
        Assert.False(File.Exists(tempPath));
    }

    private static bool HasUtf8Bom(byte[] bytes)
    {
        return bytes.Length >= 3
               && bytes[0] == 0xEF
               && bytes[1] == 0xBB
               && bytes[2] == 0xBF;
    }

    private sealed class TempDirectoryScope : IDisposable
    {
        public TempDirectoryScope()
        {
            DirectoryPath = Path.Combine(
                Path.GetTempPath(),
                "OsConverterMasterWritePathTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirectoryPath);
        }

        public string DirectoryPath { get; }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
    }
}
