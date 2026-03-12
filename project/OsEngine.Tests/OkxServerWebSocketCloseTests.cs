#nullable enable

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity.WebSocketOsEngine;
using OsEngine.Language;
using OsEngine.Logging;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.OKX;

namespace OsEngine.Tests;

public sealed class OkxServerWebSocketCloseTests
{
    [Fact]
    public void HandleWebSocketClosed_WithPublicPath_ShouldDisconnectAndLogBaseMessage()
    {
        OkxServerRealization realization = CreateRealization();
        realization.ServerStatus = ServerConnectStatus.Connect;

        int disconnectCount = 0;
        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.DisconnectEvent += () => disconnectCount++;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        bool result = InvokeHandleWebSocketClosed(
            realization,
            new CloseEventArgs { Code = "1000", Reason = "Normal Closure" },
            includeServerReason: false);

        Assert.True(result);
        Assert.Equal(ServerConnectStatus.Disconnect, realization.ServerStatus);
        Assert.Equal(1, disconnectCount);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.Equal(
            typeof(OkxServerRealization).Name + OsLocalization.Market.Message101 + "\n" + OsLocalization.Market.Message102,
            loggedMessage);
    }

    [Fact]
    public void HandleWebSocketClosed_WithPrivatePath_ShouldAppendServerReason()
    {
        OkxServerRealization realization = CreateRealization();
        realization.ServerStatus = ServerConnectStatus.Connect;

        int disconnectCount = 0;
        string? loggedMessage = null;
        realization.DisconnectEvent += () => disconnectCount++;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        bool result = InvokeHandleWebSocketClosed(
            realization,
            new CloseEventArgs { Code = "1006", Reason = "Abnormal Closure" },
            includeServerReason: true);

        Assert.True(result);
        Assert.Equal(ServerConnectStatus.Disconnect, realization.ServerStatus);
        Assert.Equal(1, disconnectCount);
        Assert.Equal(
            typeof(OkxServerRealization).Name + OsLocalization.Market.Message101 + "\n" + OsLocalization.Market.Message102 + "Server: 1006 Abnormal Closure",
            loggedMessage);
    }

    [Fact]
    public void HandleWebSocketClosed_WhenAlreadyDisconnected_ShouldReturnFalseWithoutLogging()
    {
        OkxServerRealization realization = CreateRealization();
        realization.ServerStatus = ServerConnectStatus.Disconnect;

        int disconnectCount = 0;
        string? loggedMessage = null;
        realization.DisconnectEvent += () => disconnectCount++;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        bool result = InvokeHandleWebSocketClosed(
            realization,
            new CloseEventArgs { Code = "1000", Reason = "Normal Closure" },
            includeServerReason: true);

        Assert.False(result);
        Assert.Equal(ServerConnectStatus.Disconnect, realization.ServerStatus);
        Assert.Equal(0, disconnectCount);
        Assert.Null(loggedMessage);
    }

    private static OkxServerRealization CreateRealization()
    {
        return (OkxServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(OkxServerRealization));
    }

    private static bool InvokeHandleWebSocketClosed(
        OkxServerRealization realization,
        CloseEventArgs args,
        bool includeServerReason)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "HandleWebSocketClosed",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("HandleWebSocketClosed method not found.");

        return (bool)(method.Invoke(realization, [args, includeServerReason])
            ?? throw new InvalidOperationException("HandleWebSocketClosed returned null."));
    }
}
