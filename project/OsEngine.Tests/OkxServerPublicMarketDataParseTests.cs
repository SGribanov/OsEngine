#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using OsEngine.Market.Servers.OKX;
using OsEngine.Market.Servers.OKX.Entity;

namespace OsEngine.Tests;

public sealed class OkxServerPublicMarketDataParseTests
{
    [Fact]
    public void ParsePublicCandlesResponse_ShouldParseCandlesPayload()
    {
        const string responseBody = "{\"code\":\"0\",\"msg\":\"\",\"data\":[[\"1710000000000\",\"50000.1\",\"50100.2\"]]}";

        CandlesResponse response = InvokeParsePublicCandlesResponse(responseBody);

        Assert.Equal("0", response.code);
        Assert.Equal(string.Empty, response.msg);
        List<string> candle = Assert.Single(response.data);
        Assert.Equal(["1710000000000", "50000.1", "50100.2"], candle);
    }

    [Fact]
    public void ParsePublicTradesDataResponse_ShouldParseTradesPayload()
    {
        const string responseBody = "{\"code\":\"0\",\"msg\":\"\",\"data\":[{\"instId\":\"BTC-USDT-SWAP\",\"side\":\"Sell\",\"sz\":\"2\",\"px\":\"50200.5\",\"tradeId\":\"123\",\"ts\":\"1710000000000\"}]}";

        TradesDataResponse response = InvokeParsePublicTradesDataResponse(responseBody);

        Assert.Equal("0", response.code);
        TradeData trade = Assert.Single(response.data);
        Assert.Equal("BTC-USDT-SWAP", trade.instId);
        Assert.Equal("Sell", trade.side);
        Assert.Equal("2", trade.sz);
        Assert.Equal("50200.5", trade.px);
        Assert.Equal("123", trade.tradeId);
        Assert.Equal("1710000000000", trade.ts);
    }

    [Fact]
    public void ParsePublicFundingHistoryResponse_ShouldParseFundingPayload()
    {
        const string responseBody = "{\"code\":\"0\",\"msg\":\"\",\"data\":[{\"instId\":\"BTC-USDT-SWAP\",\"fundingTime\":\"1710000000000\",\"fundingRate\":\"0.0001\"}],\"inTime\":\"1\",\"outTime\":\"2\"}";

        ResponseRestMessage<List<FundingItemHistory>> response = InvokeParsePublicFundingHistoryResponse(responseBody);

        Assert.Equal("0", response.code);
        Assert.Equal("1", response.inTime);
        Assert.Equal("2", response.outTime);
        FundingItemHistory item = Assert.Single(response.data);
        Assert.Equal("BTC-USDT-SWAP", item.instId);
        Assert.Equal("1710000000000", item.fundingTime);
        Assert.Equal("0.0001", item.fundingRate);
    }

    private static CandlesResponse InvokeParsePublicCandlesResponse(string responseContent)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ParsePublicCandlesResponse",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ParsePublicCandlesResponse method not found.");

        return (CandlesResponse)(method.Invoke(null, [responseContent])
            ?? throw new InvalidOperationException("ParsePublicCandlesResponse returned null."));
    }

    private static TradesDataResponse InvokeParsePublicTradesDataResponse(string responseContent)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ParsePublicTradesDataResponse",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ParsePublicTradesDataResponse method not found.");

        return (TradesDataResponse)(method.Invoke(null, [responseContent])
            ?? throw new InvalidOperationException("ParsePublicTradesDataResponse returned null."));
    }

    private static ResponseRestMessage<List<FundingItemHistory>> InvokeParsePublicFundingHistoryResponse(string responseContent)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ParsePublicFundingHistoryResponse",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ParsePublicFundingHistoryResponse method not found.");

        return (ResponseRestMessage<List<FundingItemHistory>>)(method.Invoke(null, [responseContent])
            ?? throw new InvalidOperationException("ParsePublicFundingHistoryResponse returned null."));
    }
}
