#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
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
