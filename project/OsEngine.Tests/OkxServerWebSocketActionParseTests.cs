#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using OsEngine.Market.Servers.OKX;
using OsEngine.Market.Servers.OKX.Entity;

namespace OsEngine.Tests;

public sealed class OkxServerWebSocketActionParseTests
{
    [Fact]
    public void ParseWebSocketActionResponse_WithObjectPayload_ShouldParseEnvelope()
    {
        const string responseBody = "{\"event\":\"subscribe\",\"msg\":\"ok\",\"arg\":{\"channel\":\"tickers\",\"instId\":\"BTC-USDT-SWAP\"},\"data\":{}}";

        ResponseWsMessageAction<object> response = InvokeParseWebSocketActionResponse<object>(responseBody);

        Assert.Equal("subscribe", response.@event);
        Assert.Equal("ok", response.msg);
        Assert.NotNull(response.arg);
        Assert.Equal("tickers", response.arg.channel);
        Assert.Equal("BTC-USDT-SWAP", response.arg.instId);
        Assert.NotNull(response.data);
    }

    [Fact]
    public void ParseWebSocketActionResponse_WithTypedPayload_ShouldParseDataItems()
    {
        const string responseBody = "{\"arg\":{\"channel\":\"orders\",\"instId\":\"BTC-USDT-SWAP\"},\"data\":[{\"ordId\":\"42\",\"instId\":\"BTC-USDT-SWAP\",\"state\":\"live\",\"side\":\"buy\"}]}";

        ResponseWsMessageAction<List<ResponseWsOrders>> response = InvokeParseWebSocketActionResponse<List<ResponseWsOrders>>(responseBody);

        Assert.NotNull(response.arg);
        Assert.Equal("orders", response.arg.channel);
        ResponseWsOrders order = Assert.Single(response.data);
        Assert.Equal("42", order.ordId);
        Assert.Equal("BTC-USDT-SWAP", order.instId);
        Assert.Equal("live", order.state);
        Assert.Equal("buy", order.side);
    }

    private static ResponseWsMessageAction<T> InvokeParseWebSocketActionResponse<T>(string message)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ParseWebSocketActionResponse",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ParseWebSocketActionResponse method not found.");

        MethodInfo genericMethod = method.MakeGenericMethod(typeof(T));

        return (ResponseWsMessageAction<T>)(genericMethod.Invoke(null, [message])
            ?? throw new InvalidOperationException("ParseWebSocketActionResponse returned null."));
    }
}
