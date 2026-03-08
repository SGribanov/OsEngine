#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Logging;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public class LogPersistenceTests
{
    [Fact]
    public void SerializeAndAppendSafePath_ShouldStayCompatibleWithLoadMessageFromLastDay()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-log-persistence-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        try
        {
            using (CurrentDirectoryScope scope = new CurrentDirectoryScope(root))
            {
                LogMessage first = new LogMessage
                {
                    Time = new DateTime(2026, 3, 8, 10, 11, 12, DateTimeKind.Local),
                    Type = LogMessageType.Signal,
                    Message = "first-entry"
                };
                LogMessage second = new LogMessage
                {
                    Time = new DateTime(2026, 3, 8, 10, 11, 13, DateTimeKind.Local),
                    Type = LogMessageType.Error,
                    Message = "second-entry"
                };
                LogMessage third = new LogMessage
                {
                    Time = new DateTime(2026, 3, 8, 10, 11, 14, DateTimeKind.Local),
                    Type = LogMessageType.Trade,
                    Message = "third-entry"
                };

                string path = Path.Combine(root, "Engine", "Log", GetCurrentDayFileName("Codex"));
                string firstLine = InvokeStatic<string>("SerializeLogMessage", first);
                string secondLine = InvokeStatic<string>("SerializeLogMessage", second);
                string thirdLine = InvokeStatic<string>("SerializeLogMessage", third);

                AssertLogLineShape(firstLine, "Signal", "first-entry");

                InvokeStatic<object?>("AppendLogMessagesSafely", path, new List<LogMessage> { first, second });
                InvokeStatic<object?>("AppendLogMessagesSafely", path, new List<LogMessage> { third });

                Assert.Equal(new[] { firstLine, secondLine, thirdLine }, File.ReadAllLines(path));
                Assert.True(File.Exists(path + ".bak"));
                Assert.Equal(new[] { firstLine, secondLine }, File.ReadAllLines(path + ".bak"));

                Log log = CreateLogWithoutConstructor("Codex");
                List<LogMessage>? loaded = log.LoadMessageFromLastDay();

                Assert.NotNull(loaded);
                Assert.Equal(3, loaded!.Count);
                Assert.Equal(LogMessageType.OldSession, loaded[0].Type);
                Assert.Equal("Signal first-entry", loaded[0].Message);
                Assert.Equal("Error second-entry", loaded[1].Message);
                Assert.Equal("Trade third-entry", loaded[2].Message);
            }
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private static void AssertLogLineShape(string line, string expectedType, string expectedMessage)
    {
        string[] parts = line.Split(';');

        Assert.Equal(4, parts.Length);
        Assert.Equal(expectedType, parts[1]);
        Assert.Equal(expectedMessage, parts[2]);
        Assert.Equal(string.Empty, parts[3]);
    }

    private static Log CreateLogWithoutConstructor(string uniqName)
    {
        Log log = (Log)RuntimeHelpers.GetUninitializedObject(typeof(Log));
        SetPrivateField(log, "_uniqName", uniqName);
        return log;
    }

    private static string GetCurrentDayFileName(string uniqName)
    {
        DateTime now = DateTime.Now;
        return uniqName + "Log_" + now.Year + "_" + now.Month + "_" + now.Day + ".txt";
    }

    private static T InvokeStatic<T>(string methodName, params object[] args)
    {
        MethodInfo? method = typeof(Log).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

        if (method == null)
        {
            throw new InvalidOperationException("Method not found: " + methodName);
        }

        object? result = method.Invoke(null, args);
        return (T)result!;
    }

    private static void SetPrivateField(object target, string fieldName, object? value)
    {
        FieldInfo? field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        if (field == null)
        {
            throw new InvalidOperationException("Field not found: " + fieldName);
        }

        field.SetValue(target, value);
    }

    private sealed class CurrentDirectoryScope : IDisposable
    {
        private readonly string _originalDirectory;

        public CurrentDirectoryScope(string newDirectory)
        {
            _originalDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(newDirectory);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(_originalDirectory);
        }
    }
}
