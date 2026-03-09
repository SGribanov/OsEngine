#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public sealed class MainWindowCheckWorkDirectoryTests
{
    [Fact]
    public void CheckWorkWithDirectory_ShouldCreateProbeFile_AndReleaseHandle()
    {
        using EngineDirectoryScope scope = new();
        OsEngine.MainWindow mainWindow = CreateMainWindowWithoutConstructor();
        MethodInfo checkMethod = typeof(OsEngine.MainWindow).GetMethod("CheckWorkWithDirectory", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method CheckWorkWithDirectory not found.");

        bool result = scope.RunInScope(() => (bool)checkMethod.Invoke(mainWindow, null)!);

        Assert.True(result);
        Assert.True(File.Exists(scope.CheckFilePath));

        scope.RunActionInScope(() =>
        {
            using FileStream overwriteProbe = new FileStream(scope.CheckFilePath, FileMode.Open, FileAccess.Write, FileShare.None);
            overwriteProbe.WriteByte(1);
        });
    }

    [Fact]
    public void ProbeEngineDirectoryWriteability_ShouldReturnFalse_WhenTargetPathIsBlockedByFile()
    {
        using EngineDirectoryScope scope = new();
        OsEngine.MainWindow mainWindow = CreateMainWindowWithoutConstructor();
        MethodInfo checkMethod = typeof(OsEngine.MainWindow).GetMethod("CheckWorkWithDirectory", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method CheckWorkWithDirectory not found.");

        scope.RunActionInScope(() =>
        {
            if (Directory.Exists("Engine"))
            {
                Directory.Delete("Engine", recursive: true);
            }

            File.WriteAllText("Engine", "occupied");
        });

        bool result = scope.RunInScope(() => (bool)checkMethod.Invoke(mainWindow, null)!);

        Assert.False(result);
    }

    private static OsEngine.MainWindow CreateMainWindowWithoutConstructor()
    {
        return (OsEngine.MainWindow)RuntimeHelpers.GetUninitializedObject(typeof(OsEngine.MainWindow));
    }

    private sealed class EngineDirectoryScope : IDisposable
    {
        private readonly string _rootPath;
        private readonly string _originalCurrentDirectory;

        public EngineDirectoryScope()
        {
            _rootPath = Path.Combine(Path.GetTempPath(), "osengine-mainwindow-check-" + Guid.NewGuid());
            Directory.CreateDirectory(_rootPath);
            _originalCurrentDirectory = Environment.CurrentDirectory;
        }

        public string CheckFilePath => Path.Combine(_rootPath, "Engine", "checkFile.txt");

        public T RunInScope<T>(Func<T> action)
        {
            Environment.CurrentDirectory = _rootPath;

            try
            {
                return action();
            }
            finally
            {
                Environment.CurrentDirectory = _originalCurrentDirectory;
            }
        }

        public void RunActionInScope(Action action)
        {
            Environment.CurrentDirectory = _rootPath;

            try
            {
                action();
            }
            finally
            {
                Environment.CurrentDirectory = _originalCurrentDirectory;
            }
        }

        public void Dispose()
        {
            Environment.CurrentDirectory = _originalCurrentDirectory;

            if (Directory.Exists(_rootPath))
            {
                Directory.Delete(_rootPath, recursive: true);
            }
        }
    }
}
