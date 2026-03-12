#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using OsEngine.Market.Servers.OKX;
using OsEngine.Market.Servers.OKX.Entity;

namespace OsEngine.Tests;

public sealed class OkxServerWebSocketPayloadGuardTests
{
    [Fact]
    public void HasWebSocketListPayloadItems_WithNullData_ShouldReturnFalse()
    {
        ResponseWsMessageAction<List<ResponseWsGreeks>> response = new ResponseWsMessageAction<List<ResponseWsGreeks>>
        {
            data = null!,
        };

        bool result = InvokeHasWebSocketListPayloadItems(response);

        Assert.False(result);
    }

    [Fact]
    public void HasWebSocketListPayloadItems_WithEmptyData_ShouldReturnFalse()
    {
        ResponseWsMessageAction<List<ResponseWsGreeks>> response = new ResponseWsMessageAction<List<ResponseWsGreeks>>
        {
            data = new List<ResponseWsGreeks>(),
        };

        bool result = InvokeHasWebSocketListPayloadItems(response);

        Assert.False(result);
    }

    [Fact]
    public void HasWebSocketListPayloadItems_WithItems_ShouldReturnTrue()
    {
        ResponseWsMessageAction<List<ResponseWsGreeks>> response = new ResponseWsMessageAction<List<ResponseWsGreeks>>
        {
            data = new List<ResponseWsGreeks> { new ResponseWsGreeks { instId = "BTC-USD-240329-50000-C" } },
        };

        bool result = InvokeHasWebSocketListPayloadItems(response);

        Assert.True(result);
    }

    private static bool InvokeHasWebSocketListPayloadItems<T>(ResponseWsMessageAction<List<T>> response)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "HasWebSocketListPayloadItems",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("HasWebSocketListPayloadItems method not found.");

        MethodInfo genericMethod = method.MakeGenericMethod(typeof(T));

        return (bool)(genericMethod.Invoke(null, [response])
            ?? throw new InvalidOperationException("HasWebSocketListPayloadItems returned null."));
    }
}
