#nullable enable

using System;
using System.IO;
using System.Reflection;
using OsEngine.Market.Servers.Finam.Entity;

namespace OsEngine.Tests;

public sealed class FinamDataSeriesTempWriteTests : IDisposable
{
    private readonly string _tempDirectory;

    public FinamDataSeriesTempWriteTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "osengine-finam-temp-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void PersistTempFileContent_ShouldWriteBytesAndRemoveStagingFile()
    {
        string filePath = Path.Combine(_tempDirectory, "trades.txt");
        byte[] expectedBytes = [1, 2, 3, 4, 5];

        using MemoryStream stream = new MemoryStream(expectedBytes);

        InvokePersistTempFileContent(stream, filePath);

        Assert.True(File.Exists(filePath));
        Assert.Equal(expectedBytes, File.ReadAllBytes(filePath));
        Assert.False(File.Exists(filePath + ".download"));
    }

    [Fact]
    public void PersistTempFileContent_WhenCopyFails_ShouldDeleteStagingFile()
    {
        string filePath = Path.Combine(_tempDirectory, "broken.txt");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => InvokePersistTempFileContent(new ThrowingReadStream(), filePath));

        Assert.Equal("Simulated copy failure.", exception.Message);
        Assert.False(File.Exists(filePath));
        Assert.False(File.Exists(filePath + ".download"));
    }

    private static void InvokePersistTempFileContent(Stream contentStream, string filePath)
    {
        MethodInfo method = typeof(FinamDataSeries).GetMethod(
            "PersistTempFileContent",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("PersistTempFileContent method not found.");

        try
        {
            method.Invoke(null, [contentStream, filePath]);
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
                buffer[offset] = 42;
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
