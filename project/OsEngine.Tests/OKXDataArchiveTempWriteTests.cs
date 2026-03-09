#nullable enable

using System;
using System.IO;
using System.Reflection;
using OsEngine.Market.Servers.OKXData;

namespace OsEngine.Tests;

public sealed class OKXDataArchiveTempWriteTests : IDisposable
{
    private readonly string _tempDirectory;

    public OKXDataArchiveTempWriteTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "osengine-okx-archive-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void PersistArchiveContentToTempFile_ShouldWriteBytesAndCleanStagingFile()
    {
        string archivePath = Path.Combine(_tempDirectory, "archive.zip");
        byte[] expectedBytes = [1, 2, 3, 4, 5, 6];

        using MemoryStream stream = new MemoryStream(expectedBytes);

        InvokePersistArchiveContentToTempFile(stream, archivePath);

        Assert.True(File.Exists(archivePath));
        Assert.Equal(expectedBytes, File.ReadAllBytes(archivePath));
        Assert.False(File.Exists(archivePath + ".download"));
    }

    [Fact]
    public void PersistArchiveContentToTempFile_WhenCopyFails_ShouldDeleteStagingFile()
    {
        string archivePath = Path.Combine(_tempDirectory, "broken.zip");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => InvokePersistArchiveContentToTempFile(new ThrowingReadStream(), archivePath));

        Assert.Equal("Simulated copy failure.", exception.Message);
        Assert.False(File.Exists(archivePath));
        Assert.False(File.Exists(archivePath + ".download"));
    }

    private static void InvokePersistArchiveContentToTempFile(Stream stream, string archivePath)
    {
        MethodInfo method = typeof(OKXDataServerRealization).GetMethod(
            "PersistArchiveContentToTempFile",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("PersistArchiveContentToTempFile method not found.");

        try
        {
            method.Invoke(null, [stream, archivePath]);
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
