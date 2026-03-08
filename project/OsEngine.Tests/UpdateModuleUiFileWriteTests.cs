#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OsEngine.UpdateModule;

namespace OsEngine.Tests;

public class UpdateModuleUiFileWriteTests
{
    [Fact]
    public void BuildFileListLines_ShouldPreserveOrderAndNames()
    {
        List<FileState> files =
        [
            new FileState { Name = @"Engine\NewA.dll", State = State.New },
            new FileState { Name = @"Custom\Robot.cs", State = State.Removed }
        ];

        string[] lines = InvokeStatic<string[]>("BuildFileListLines", files);

        Assert.Equal(new[] { @"Engine\NewA.dll", @"Custom\Robot.cs" }, lines);
    }

    [Fact]
    public void BuildFilesVersionsTimeLines_ShouldPreserveExistingFormat()
    {
        DateTime timestamp = new DateTime(2026, 03, 08, 14, 15, 16);
        List<GithubFileInfo> files =
        [
            new GithubFileInfo
            {
                Name = @"Engine\Updater.exe",
                LastUpdate = timestamp,
                Size = 321
            }
        ];

        string[] lines = InvokeStatic<string[]>("BuildFilesVersionsTimeLines", files);

        Assert.Equal(new[] { @"Engine\Updater.exe#" + timestamp + "#321" }, lines);
    }

    [Fact]
    public void AppendUpdaterLogMessage_ShouldAppendAndCreateBackup()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-updater-log-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        try
        {
            string path = Path.Combine(root, "UpdaterLog.txt");
            File.WriteAllText(path, "first\n");

            InvokeStatic<object?>("AppendUpdaterLogMessage", path, "second\n");

            Assert.Equal("first\nsecond\n", File.ReadAllText(path));
            Assert.True(File.Exists(path + ".bak"));
            Assert.Equal("first\n", File.ReadAllText(path + ".bak"));
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

    private static T InvokeStatic<T>(string methodName, params object[] args)
    {
        MethodInfo? method = typeof(UpdateModuleUi).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

        if (method == null)
        {
            throw new InvalidOperationException("Method not found: " + methodName);
        }

        object? result = method.Invoke(null, args);

        return (T)result!;
    }
}
