using System;
using System.IO;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class SafeFileWriterTests
{
    [Fact]
    public void WriteAllLines_NewFile_ShouldCreateTargetWithoutTempFile()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-safe-writer-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        try
        {
            string path = Path.Combine(root, "settings.txt");

            SafeFileWriter.WriteAllLines(path, new[] { "line1", "line2" });

            Assert.True(File.Exists(path));
            Assert.Equal(new[] { "line1", "line2" }, File.ReadAllLines(path));
            Assert.False(File.Exists(path + ".tmp"));
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
    public void WriteAllText_ExistingFile_ShouldCreateBackupWithPreviousContent()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-safe-writer-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        try
        {
            string path = Path.Combine(root, "state.json");
            File.WriteAllText(path, "{\"version\":1}");

            SafeFileWriter.WriteAllText(path, "{\"version\":2}");

            Assert.Equal("{\"version\":2}", File.ReadAllText(path));
            Assert.True(File.Exists(path + ".bak"));
            Assert.Equal("{\"version\":1}", File.ReadAllText(path + ".bak"));
            Assert.False(File.Exists(path + ".tmp"));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
