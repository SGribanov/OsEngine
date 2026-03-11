#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using OsEngine.Market.Servers.OKX;
using OsEngine.Market.Servers.OKX.Entity;

namespace OsEngine.Tests;

public sealed class OkxServerPrivateOrderParseTests
{
    [Fact]
    public void ParsePrivateSendOrderResponse_ShouldParseSendOrderPayload()
    {
        const string responseBody = "{\"code\":\"1\",\"msg\":\"error\",\"data\":[{\"sMsg\":\"insufficient balance\"}],\"inTime\":\"1\",\"outTime\":\"2\"}";

        ResponseRestMessage<List<RestMessageSendOrder>> response = InvokeParsePrivateSendOrderResponse(responseBody);

        Assert.Equal("1", response.code);
        Assert.Equal("error", response.msg);
        Assert.Equal("1", response.inTime);
        Assert.Equal("2", response.outTime);
        RestMessageSendOrder item = Assert.Single(response.data);
        Assert.Equal("insufficient balance", item.sMsg);
    }

    [Fact]
    public void ParsePrivateOrdersResponse_ShouldParseOrdersPayload()
    {
        const string responseBody = "{\"code\":\"0\",\"msg\":\"\",\"data\":[{\"ordId\":\"42\",\"instId\":\"BTC-USDT-SWAP\",\"state\":\"live\",\"side\":\"buy\"}],\"inTime\":\"3\",\"outTime\":\"4\"}";

        ResponseRestMessage<List<ResponseWsOrders>> response = InvokeParsePrivateOrdersResponse(responseBody);

        Assert.Equal("0", response.code);
        Assert.Equal("3", response.inTime);
        Assert.Equal("4", response.outTime);
        ResponseWsOrders item = Assert.Single(response.data);
        Assert.Equal("42", item.ordId);
        Assert.Equal("BTC-USDT-SWAP", item.instId);
        Assert.Equal("live", item.state);
        Assert.Equal("buy", item.side);
    }

    private static ResponseRestMessage<List<RestMessageSendOrder>> InvokeParsePrivateSendOrderResponse(string responseContent)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ParsePrivateSendOrderResponse",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ParsePrivateSendOrderResponse method not found.");

        return (ResponseRestMessage<List<RestMessageSendOrder>>)(method.Invoke(null, [responseContent])
            ?? throw new InvalidOperationException("ParsePrivateSendOrderResponse returned null."));
    }

    private static ResponseRestMessage<List<ResponseWsOrders>> InvokeParsePrivateOrdersResponse(string responseContent)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ParsePrivateOrdersResponse",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ParsePrivateOrdersResponse method not found.");

        return (ResponseRestMessage<List<ResponseWsOrders>>)(method.Invoke(null, [responseContent])
            ?? throw new InvalidOperationException("ParsePrivateOrdersResponse returned null."));
    }
}
