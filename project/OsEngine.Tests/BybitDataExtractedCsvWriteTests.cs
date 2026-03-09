#nullable enable

using System;
using System.IO;
using System.Reflection;
using OsEngine.Market.Servers.BybitData;

namespace OsEngine.Tests;

public sealed class BybitDataExtractedCsvWriteTests : IDisposable
{
    private readonly string _tempDirectory;

    public BybitDataExtractedCsvWriteTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "osengine-bybit-extract-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void PersistExtractedCsvContent_ShouldWriteBytesAndCleanStagingFile()
    {
        string csvPath = Path.Combine(_tempDirectory, "temp.csv");
        byte[] expectedBytes = [11, 22, 33, 44];

        using MemoryStream stream = new MemoryStream(expectedBytes);

        InvokePersistExtractedCsvContent(stream, csvPath);

        Assert.True(File.Exists(csvPath));
        Assert.Equal(expectedBytes, File.ReadAllBytes(csvPath));
        Assert.False(File.Exists(csvPath + ".tmp"));
    }

    [Fact]
    public void PersistExtractedCsvContent_WhenCopyFails_ShouldDeleteStagingFile()
    {
        string csvPath = Path.Combine(_tempDirectory, "broken.csv");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => InvokePersistExtractedCsvContent(new ThrowingReadStream(), csvPath));

        Assert.Equal("Simulated copy failure.", exception.Message);
        Assert.False(File.Exists(csvPath));
        Assert.False(File.Exists(csvPath + ".tmp"));
    }

    private static void InvokePersistExtractedCsvContent(Stream stream, string csvPath)
    {
        MethodInfo method = typeof(BybitDataServerRealization).GetMethod(
            "PersistExtractedCsvContent",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("PersistExtractedCsvContent method not found.");

        try
        {
            method.Invoke(null, [stream, csvPath]);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    private sealed class ThrowingReadStream : Stream
    {
        private int _readCount;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_readCount == 0)
            {
                buffer[offset] = 99;
                _readCount++;
                return 1;
            }

            throw new InvalidOperationException("Simulated copy failure.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
