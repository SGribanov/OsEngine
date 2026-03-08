#nullable enable
using System;
using System.Reflection;
using OsEngine.Logging;

namespace OsEngine.Tests;

public class ServerTelegramUiTests
{
    [Fact]
    public void TryReadSettings_ShouldParseValidInvariantIntegerAndCheckbox()
    {
        object?[] args = ["bot-token", "123456789", true, null!, 0L, false];

        bool parsed = InvokeStatic<bool>("TryReadSettings", args);

        Assert.True(parsed);
        Assert.Equal("bot-token", args[3]);
        Assert.Equal(123456789L, (long)args[4]!);
        Assert.True((bool)args[5]!);
    }

    [Fact]
    public void TryReadSettings_ShouldRejectInvalidTextAndDefaultCheckboxToFalse()
    {
        object?[] args = [null, "not-a-chat-id", null, null!, 777L, true];

        bool parsed = InvokeStatic<bool>("TryReadSettings", args);

        Assert.False(parsed);
        Assert.Equal(string.Empty, args[3]);
        Assert.Equal(0L, (long)args[4]!);
        Assert.False((bool)args[5]!);
    }

    private static T InvokeStatic<T>(string methodName, object?[] args)
    {
        MethodInfo method = typeof(ServerTelegramDeliveryUi).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method not found: " + methodName);

        object? result = method.Invoke(null, args);

        return (T)result!;
    }
}
