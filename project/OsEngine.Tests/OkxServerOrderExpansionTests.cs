#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.Market.Servers.OKX;
using OsEngine.Market.Servers.OKX.Entity;

namespace OsEngine.Tests;

public sealed class OkxServerOrderExpansionTests
{
    [Fact]
    public void AppendOrdersFromPrivateResponse_ShouldFilterUnsupportedOrdTypesAndTrimToMaxCount()
    {
        OkxServerRealization realization = CreateRealization();
        List<Order> orders = new List<Order>();
        List<ResponseWsOrders> responseData =
        [
            CreateResponseOrder("1", "limit", "buy"),
            CreateResponseOrder("2", "market", "sell"),
            CreateResponseOrder("3", "post_only", "buy"),
        ];

        InvokeAppendOrdersFromPrivateResponse(realization, orders, responseData, 1);

        Order order = Assert.Single(orders);
        Assert.Equal("1", order.NumberMarket);
        Assert.Equal(OrderPriceType.Limit, order.TypeOrder);
        Assert.Equal(Side.Buy, order.Side);
    }

    [Fact]
    public void AppendOrdersFromPrivateResponse_WithOnlyUnsupportedOrders_ShouldLeaveArrayUnchanged()
    {
        OkxServerRealization realization = CreateRealization();
        List<Order> orders = new List<Order>();
        List<ResponseWsOrders> responseData =
        [
            CreateResponseOrder("3", "post_only", "buy"),
        ];

        InvokeAppendOrdersFromPrivateResponse(realization, orders, responseData, 5);

        Assert.Empty(orders);
    }

    [Fact]
    public void ResolvePrivateOrdersQueryResult_WithSuccessCode_ShouldAppendOrdersWithoutLogging()
    {
        OkxServerRealization realization = CreateRealization();
        List<Order> orders = new List<Order>();
        string? loggedMessage = null;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        InvokeResolvePrivateOrdersQueryResult(
            realization,
            orders,
            5,
            HttpStatusCode.OK,
            "ignored",
            new ResponseRestMessage<List<ResponseWsOrders>>
            {
                code = "0",
                data = new List<ResponseWsOrders> { CreateResponseOrder("1", "limit", "buy") },
            },
            static response => $"API {response.code}",
            static (statusCode, content) => $"HTTP {statusCode} {content}");

        Order order = Assert.Single(orders);
        Assert.Equal("1", order.NumberMarket);
        Assert.Null(loggedMessage);
    }

    [Fact]
    public void ResolvePrivateOrdersQueryResult_WithApiError_ShouldLogApiFormatter()
    {
        OkxServerRealization realization = CreateRealization();
        List<Order> orders = new List<Order>();
        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        InvokeResolvePrivateOrdersQueryResult(
            realization,
            orders,
            5,
            HttpStatusCode.OK,
            "ignored",
            new ResponseRestMessage<List<ResponseWsOrders>>
            {
                code = "1",
                msg = "bad request",
                data = new List<ResponseWsOrders>(),
            },
            static response => $"API {response.code} {response.msg}",
            static (statusCode, content) => $"HTTP {statusCode} {content}");

        Assert.Empty(orders);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.Equal("API 1 bad request", loggedMessage);
    }

    [Fact]
    public void ResolvePrivateOrdersQueryResult_WithTransportError_ShouldLogTransportFormatter()
    {
        OkxServerRealization realization = CreateRealization();
        List<Order> orders = new List<Order>();
        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        InvokeResolvePrivateOrdersQueryResult(
            realization,
            orders,
            5,
            HttpStatusCode.BadGateway,
            "gateway down",
            new ResponseRestMessage<List<ResponseWsOrders>>
            {
                code = "0",
                data = new List<ResponseWsOrders>(),
            },
            static response => $"API {response.code}",
            static (statusCode, content) => $"HTTP {statusCode} {content}");

        Assert.Empty(orders);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.Equal("HTTP BadGateway gateway down", loggedMessage);
    }

    private static OkxServerRealization CreateRealization()
    {
        return (OkxServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(OkxServerRealization));
    }

    private static void InvokeAppendOrdersFromPrivateResponse(
        OkxServerRealization realization,
        List<Order> orders,
        List<ResponseWsOrders> responseData,
        int maxCount)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "AppendOrdersFromPrivateResponse",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("AppendOrdersFromPrivateResponse method not found.");

        method.Invoke(realization, [orders, responseData, maxCount]);
    }

    private static void InvokeResolvePrivateOrdersQueryResult(
        OkxServerRealization realization,
        List<Order> orders,
        int maxCount,
        HttpStatusCode statusCode,
        string contentStr,
        ResponseRestMessage<List<ResponseWsOrders>> response,
        Func<ResponseRestMessage<List<ResponseWsOrders>>, string> apiErrorMessageFactory,
        Func<HttpStatusCode, string, string> transportErrorMessageFactory)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ResolvePrivateOrdersQueryResult",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ResolvePrivateOrdersQueryResult method not found.");

        method.Invoke(realization, [orders, maxCount, statusCode, contentStr, response, apiErrorMessageFactory, transportErrorMessageFactory]);
    }

    private static ResponseWsOrders CreateResponseOrder(string ordId, string ordType, string side)
    {
        return new ResponseWsOrders
        {
            ordId = ordId,
            instId = "BTC-USDT",
            state = "live",
            side = side,
            ordType = ordType,
            cTime = "1700000000000",
            uTime = "1700000001000",
            clOrdId = ordId,
            sz = "1",
            px = "50000",
        };
    }
}
