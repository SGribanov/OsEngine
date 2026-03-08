#nullable enable
using System;
using System.Reflection;
using OsEngine.Logging;

namespace OsEngine.Tests;

public class ServerTelegramUiTests
{
    [Fact]
    public void TryParseChatId_ShouldParseValidInvariantInteger()
    {
        object[] args = ["123456789", 0L];

        bool parsed = InvokeStatic<bool>("TryParseChatId", args);

        Assert.True(parsed);
        Assert.Equal(123456789L, (long)args[1]);
    }

    [Fact]
    public void TryParseChatId_ShouldRejectInvalidText()
    {
        object[] args = ["not-a-chat-id", 0L];

        bool parsed = InvokeStatic<bool>("TryParseChatId", args);

        Assert.False(parsed);
    }

    private static T InvokeStatic<T>(string methodName, object[] args)
    {
        MethodInfo method = typeof(ServerTelegramDeliveryUi).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method not found: " + methodName);

        object? result = method.Invoke(null, args);

        return (T)result!;
    }
}
