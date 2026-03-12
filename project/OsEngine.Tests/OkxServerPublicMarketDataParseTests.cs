#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

    [Fact]
    public void ExecutePublicAbsoluteQueryRequest_ShouldReturnParsedValueFromResponseContent()
    {
        using LocalHttpServer server = new LocalHttpServer(HttpStatusCode.OK, "{\"probe\":\"ok\"}");

        (HttpStatusCode statusCode, string result) = InvokeExecutePublicAbsoluteQueryRequest(
            $"{server.BaseUrl}/api/v5/market/candles?instId=BTC-USDT-SWAP",
            static content => "PROBE: " + content);

        Assert.Equal(HttpStatusCode.OK, statusCode);
        Assert.Equal("PROBE: {\"probe\":\"ok\"}", result);
        Assert.Equal("/api/v5/market/candles?instId=BTC-USDT-SWAP", server.LastRequestPath);
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

    private static (HttpStatusCode statusCode, string result) InvokeExecutePublicAbsoluteQueryRequest(string url, Func<string, string> parser)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecutePublicAbsoluteQueryRequest",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ExecutePublicAbsoluteQueryRequest method not found.");

        MethodInfo genericMethod = method.MakeGenericMethod(typeof(string));

        object tuple = genericMethod.Invoke(null, [url, parser])
            ?? throw new InvalidOperationException("ExecutePublicAbsoluteQueryRequest returned null.");

        Type tupleType = tuple.GetType();
        RestSharp.IRestResponse response = (RestSharp.IRestResponse)(tupleType.GetField("Item1")?.GetValue(tuple)
            ?? throw new InvalidOperationException("Tuple Item1 not found."));
        string result = (string)(tupleType.GetField("Item2")?.GetValue(tuple)
            ?? throw new InvalidOperationException("Tuple Item2 not found."));

        return (response.StatusCode, result);
    }

    private sealed class LocalHttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Task _worker;
        private readonly string _responseBody;
        private readonly HttpStatusCode _statusCode;

        public LocalHttpServer(HttpStatusCode statusCode, string responseBody)
        {
            _statusCode = statusCode;
            _responseBody = responseBody;

            int port = GetFreePort();
            BaseUrl = $"http://127.0.0.1:{port}";
            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl + "/");
            _listener.Start();
            _worker = Task.Run(HandleSingleRequestAsync);
        }

        public string BaseUrl { get; }

        public string? LastRequestPath { get; private set; }

        public void Dispose()
        {
            _listener.Stop();
            _listener.Close();
            _worker.GetAwaiter().GetResult();
        }

        private async Task HandleSingleRequestAsync()
        {
            try
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                LastRequestPath = context.Request.RawUrl;
                context.Response.StatusCode = (int)_statusCode;
                byte[] buffer = Encoding.UTF8.GetBytes(_responseBody);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
            catch (HttpListenerException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private static int GetFreePort()
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
    }
}
