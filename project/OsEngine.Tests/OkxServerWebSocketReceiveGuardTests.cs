#nullable enable

using System;
using System.Reflection;
using OsEngine.Entity.WebSocketOsEngine;
using OsEngine.Market.Servers.OKX;

namespace OsEngine.Tests;

public sealed class OkxServerWebSocketReceiveGuardTests
{
    [Fact]
    public void TryGetWebSocketMessageData_WithNullEvent_ShouldReturnFalse()
    {
        bool result = InvokeTryGetWebSocketMessageData(null, out string? messageData);

        Assert.False(result);
        Assert.Null(messageData);
    }

    [Fact]
    public void TryGetWebSocketMessageData_WithEmptyPayload_ShouldReturnFalse()
    {
        MessageEventArgs args = new MessageEventArgs
        {
            Data = string.Empty,
        };

        bool result = InvokeTryGetWebSocketMessageData(args, out string? messageData);

        Assert.False(result);
        Assert.Null(messageData);
    }

    [Fact]
    public void TryGetWebSocketMessageData_WithPongPayload_ShouldReturnFalse()
    {
        MessageEventArgs args = new MessageEventArgs
        {
            Data = "pong",
        };

        bool result = InvokeTryGetWebSocketMessageData(args, out string? messageData);

        Assert.False(result);
        Assert.Null(messageData);
    }

    [Fact]
    public void TryGetWebSocketMessageData_WithProcessablePayload_ShouldReturnTrueAndExposeData()
    {
        MessageEventArgs args = new MessageEventArgs
        {
            Data = "{\"arg\":\"books5\"}",
        };

        bool result = InvokeTryGetWebSocketMessageData(args, out string? messageData);

        Assert.True(result);
        Assert.Equal("{\"arg\":\"books5\"}", messageData);
    }

    private static bool InvokeTryGetWebSocketMessageData(MessageEventArgs? args, out string? messageData)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "TryGetWebSocketMessageData",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("TryGetWebSocketMessageData method not found.");

        object?[] parameters =
        [
            args,
            null,
        ];

        bool result = (bool)(method.Invoke(null, parameters)
            ?? throw new InvalidOperationException("TryGetWebSocketMessageData returned null."));

        messageData = parameters[1] as string;
        return result;
    }
}
