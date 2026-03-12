#nullable enable
#pragma warning disable CS9216

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.Market.Servers.OKX;
using OsEngine.Market.Servers.OKX.Entity;
using Xunit;

namespace OsEngine.Tests;

public class OkxServerPrivateHttpClientTests
{
    [Fact]
    public void GetPrivateHttpClient_ShouldCreateAndReuseSingleInstance()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();

        try
        {
            HttpClient firstClient = InvokeGetPrivateHttpClient(realization);
            HttpClient secondClient = InvokeGetPrivateHttpClient(realization);

            Assert.NotNull(firstClient);
            Assert.Same(firstClient, secondClient);
        }
        finally
        {
            InvokeDisposePrivateHttpClient(realization);
        }
    }

    [Fact]
    public void RecreatePrivateHttpClient_ShouldReplaceClientInstance()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();

        try
        {
            HttpClient firstClient = InvokeGetPrivateHttpClient(realization);

            SetPrivateField(realization, "_demoMode", true);
            SetPrivateField(realization, "_myProxy", new WebProxy("http://127.0.0.1:8080"));

            InvokeRecreatePrivateHttpClient(realization);

            HttpClient secondClient = InvokeGetPrivateHttpClient(realization);

            Assert.NotSame(firstClient, secondClient);
        }
        finally
        {
            InvokeDisposePrivateHttpClient(realization);
        }
    }

    [Fact]
    public void DisposePrivateHttpClient_ShouldClearFieldAndStayIdempotent()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();

        HttpClient client = InvokeGetPrivateHttpClient(realization);

        Assert.NotNull(client);

        InvokeDisposePrivateHttpClient(realization);
        Assert.Null(GetPrivateHttpClientField(realization));

        InvokeDisposePrivateHttpClient(realization);
        Assert.Null(GetPrivateHttpClientField(realization));
    }

    [Fact]
    public void CreateSignedRequest_WithBody_ShouldAttachJsonContentAndSignatureBodyOption()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();
        const string body = "{\"instId\":\"BTC-USDT\"}";

        using HttpRequestMessage request = InvokeCreateSignedRequest(
            realization,
            HttpMethod.Post,
            "https://www.okx.com/api/v5/trade/order",
            body);

        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://www.okx.com/api/v5/trade/order", request.RequestUri?.ToString());
        Assert.NotNull(request.Content);
        Assert.True(request.Options.TryGetValue(HttpInterceptor.SignatureBodyOptionKey, out string? savedBody));
        Assert.Equal(body, savedBody);
    }

    [Fact]
    public void SendPrivateRequest_WithBody_ShouldSendSignedRequestThroughConfiguredClient()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();
        ProbeHandler probe = new ProbeHandler();
        SetPrivateField(realization, "_privateHttpClient", new HttpClient(probe));

        using HttpResponseMessage response = InvokeSendPrivateRequest(
            realization,
            HttpMethod.Post,
            "https://www.okx.com/api/v5/trade/order",
            "{\"instId\":\"BTC-USDT\"}");

        HttpRequestMessage request = probe.LastRequest
            ?? throw new InvalidOperationException("Expected captured request.");

        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://www.okx.com/api/v5/trade/order", request.RequestUri?.ToString());
        Assert.True(request.Options.TryGetValue(HttpInterceptor.SignatureBodyOptionKey, out string? savedBody));
        Assert.Equal("{\"instId\":\"BTC-USDT\"}", savedBody);
        Assert.NotNull(request.Content);
    }

    [Fact]
    public void ReadPrivateResponseContent_ShouldReturnResponseBody()
    {
        using HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"code\":\"0\"}")
        };

        string content = InvokeReadPrivateResponseContent(response);

        Assert.Equal("{\"code\":\"0\"}", content);
    }

    [Fact]
    public void ExecutePrivateSendOrderRequest_ShouldReturnResponseContentAndParsedMessage()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();
        ProbeHandler probe = new ProbeHandler
        {
            ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"code\":\"1\",\"msg\":\"error\",\"data\":[{\"sMsg\":\"insufficient balance\"}]}")
            }
        };
        SetPrivateField(realization, "_privateHttpClient", new HttpClient(probe));

        (HttpResponseMessage response, string content, ResponseRestMessage<List<RestMessageSendOrder>> message) =
            InvokeExecutePrivateSendOrderRequest(realization, "https://www.okx.com/api/v5/trade/order", "{\"instId\":\"BTC-USDT\"}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"code\":\"1\",\"msg\":\"error\",\"data\":[{\"sMsg\":\"insufficient balance\"}]}", content);
        Assert.Equal("1", message.code);
        RestMessageSendOrder item = Assert.Single(message.data);
        Assert.Equal("insufficient balance", item.sMsg);
    }

    [Fact]
    public void HandlePrivateSendOrderFailure_WithApiError_ShouldFailOrderAndLogApiMessage()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();
        Order order = new Order();
        Order? reportedOrder = null;
        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.MyOrderEvent += newOrder => reportedOrder = newOrder;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        InvokeHandlePrivateSendOrderFailure(
            realization,
            order,
            HttpStatusCode.OK,
            "{\"code\":\"1\"}",
            new ResponseRestMessage<List<RestMessageSendOrder>>
            {
                code = "1",
                data = new List<RestMessageSendOrder> { new RestMessageSendOrder { sMsg = "insufficient balance" } },
            },
            "SendOrderSpot",
            "Spot Order Fail");

        Assert.Equal(OrderStateType.Fail, order.State);
        Assert.Same(order, reportedOrder);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.Equal("SendOrderSpot - insufficient balance", loggedMessage);
    }

    [Fact]
    public void HandlePrivateSendOrderFailure_WithTransportError_ShouldFailOrderAndLogStatusMessage()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();
        Order order = new Order();
        Order? reportedOrder = null;
        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.MyOrderEvent += newOrder => reportedOrder = newOrder;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        InvokeHandlePrivateSendOrderFailure(
            realization,
            order,
            HttpStatusCode.BadGateway,
            "gateway down",
            new ResponseRestMessage<List<RestMessageSendOrder>>
            {
                code = "0",
                data = new List<RestMessageSendOrder>(),
            },
            "SendOrderSwap",
            "Swap Order Fail");

        Assert.Equal(OrderStateType.Fail, order.State);
        Assert.Same(order, reportedOrder);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.Equal("Swap Order Fail. Status: BadGateway || gateway down", loggedMessage);
    }

    [Fact]
    public void ResolveCancelOrderFallback_WithNoneState_ShouldReturnFalseAndLogFailureMessage()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();
        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        bool result = InvokeResolveCancelOrderFallback(
            realization,
            OrderStateType.None,
            "Cancel Order Error. 42 || payload.");

        Assert.False(result);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.Equal("Cancel Order Error. 42 || payload.", loggedMessage);
    }

    [Fact]
    public void ResolveCancelOrderFallback_WithResolvedState_ShouldReturnTrueWithoutLogging()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();
        string? loggedMessage = null;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        bool result = InvokeResolveCancelOrderFallback(
            realization,
            OrderStateType.Active,
            "Cancel order failed. Status: BadGateway || payload");

        Assert.True(result);
        Assert.Null(loggedMessage);
    }

    [Fact]
    public void ExecutePrivateOrdersQueryRequest_ShouldReturnResponseContentAndParsedMessage()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();
        ProbeHandler probe = new ProbeHandler
        {
            ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"code\":\"0\",\"msg\":\"\",\"data\":[{\"ordId\":\"42\",\"instId\":\"BTC-USDT-SWAP\",\"state\":\"live\",\"side\":\"buy\"}]}")
            }
        };
        SetPrivateField(realization, "_privateHttpClient", new HttpClient(probe));

        (HttpResponseMessage response, string content, ResponseRestMessage<List<ResponseWsOrders>> message) =
            InvokeExecutePrivateOrdersQueryRequest(realization, "https://www.okx.com/api/v5/trade/orders-pending");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"code\":\"0\",\"msg\":\"\",\"data\":[{\"ordId\":\"42\",\"instId\":\"BTC-USDT-SWAP\",\"state\":\"live\",\"side\":\"buy\"}]}", content);
        Assert.Equal("0", message.code);
        ResponseWsOrders order = Assert.Single(message.data);
        Assert.Equal("42", order.ordId);
        Assert.Equal("BTC-USDT-SWAP", order.instId);
        Assert.Equal("live", order.state);
        Assert.Equal("buy", order.side);
    }

    [Fact]
    public void ExecutePrivateTradeDetailsQueryRequest_ShouldReturnResponseContentAndParsedMessage()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();
        ProbeHandler probe = new ProbeHandler
        {
            ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"code\":\"0\",\"msg\":\"\",\"data\":[{\"tradeId\":\"t1\",\"ordId\":\"42\",\"instId\":\"BTC-USDT-SWAP\",\"fillPx\":\"50000\"}]}")
            }
        };
        SetPrivateField(realization, "_privateHttpClient", new HttpClient(probe));

        (HttpResponseMessage response, string content, TradeDetailsResponse message) =
            InvokeExecutePrivateTradeDetailsQueryRequest(realization, "https://www.okx.com/api/v5/trade/fills-history?ordId=42");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"code\":\"0\",\"msg\":\"\",\"data\":[{\"tradeId\":\"t1\",\"ordId\":\"42\",\"instId\":\"BTC-USDT-SWAP\",\"fillPx\":\"50000\"}]}", content);
        Assert.Equal("0", message.code);
        TradeDetailsObject trade = Assert.Single(message.data);
        Assert.Equal("t1", trade.tradeId);
        Assert.Equal("42", trade.ordId);
        Assert.Equal("BTC-USDT-SWAP", trade.instId);
        Assert.Equal("50000", trade.fillPx);
    }

    [Fact]
    public void ExecutePrivateQueryRequest_ShouldReturnResponseContentAndParserResult()
    {
        OkxServerRealization realization = CreateRealizationForHttpClientTests();
        ProbeHandler probe = new ProbeHandler
        {
            ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("probe-body")
            }
        };
        SetPrivateField(realization, "_privateHttpClient", new HttpClient(probe));

        (HttpResponseMessage response, string content, string message) =
            InvokeExecutePrivateQueryRequest(realization, "https://www.okx.com/api/v5/trade/orders-pending");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("probe-body", content);
        Assert.Equal("PROBE-BODY", message);
    }

    private static HttpClient InvokeGetPrivateHttpClient(OkxServerRealization realization)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "GetPrivateHttpClient",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("GetPrivateHttpClient method not found.");

        return (HttpClient)(method.Invoke(realization, null)
            ?? throw new InvalidOperationException("GetPrivateHttpClient returned null."));
    }

    private static void InvokeRecreatePrivateHttpClient(OkxServerRealization realization)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "RecreatePrivateHttpClient",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("RecreatePrivateHttpClient method not found.");

        method.Invoke(realization, null);
    }

    private static void InvokeDisposePrivateHttpClient(OkxServerRealization realization)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "DisposePrivateHttpClient",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("DisposePrivateHttpClient method not found.");

        method.Invoke(realization, null);
    }

    private static HttpRequestMessage InvokeCreateSignedRequest(
        OkxServerRealization realization,
        HttpMethod method,
        string url,
        string? bodyJson)
    {
        MethodInfo createSignedRequest = typeof(OkxServerRealization).GetMethod(
            "CreateSignedRequest",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("CreateSignedRequest method not found.");

        return (HttpRequestMessage)(createSignedRequest.Invoke(realization, [method, url, bodyJson])
            ?? throw new InvalidOperationException("CreateSignedRequest returned null."));
    }

    private static HttpResponseMessage InvokeSendPrivateRequest(
        OkxServerRealization realization,
        HttpMethod method,
        string url,
        string? bodyJson)
    {
        MethodInfo sendPrivateRequest = typeof(OkxServerRealization).GetMethod(
            "SendPrivateRequest",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("SendPrivateRequest method not found.");

        return (HttpResponseMessage)(sendPrivateRequest.Invoke(realization, [method, url, bodyJson])
            ?? throw new InvalidOperationException("SendPrivateRequest returned null."));
    }

    private static string InvokeReadPrivateResponseContent(HttpResponseMessage response)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ReadPrivateResponseContent",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ReadPrivateResponseContent method not found.");

        return (string)(method.Invoke(null, [response])
            ?? throw new InvalidOperationException("ReadPrivateResponseContent returned null."));
    }

    private static (HttpResponseMessage response, string content, ResponseRestMessage<List<RestMessageSendOrder>> message) InvokeExecutePrivateSendOrderRequest(
        OkxServerRealization realization,
        string url,
        string bodyJson)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecutePrivateSendOrderRequest",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ExecutePrivateSendOrderRequest method not found.");

        object tuple = method.Invoke(realization, [url, bodyJson])
            ?? throw new InvalidOperationException("ExecutePrivateSendOrderRequest returned null.");

        return ((HttpResponseMessage response, string content, ResponseRestMessage<List<RestMessageSendOrder>> message))tuple;
    }

    private static void InvokeHandlePrivateSendOrderFailure(
        OkxServerRealization realization,
        Order order,
        HttpStatusCode statusCode,
        string content,
        ResponseRestMessage<List<RestMessageSendOrder>> message,
        string apiErrorPrefix,
        string statusErrorPrefix)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "HandlePrivateSendOrderFailure",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("HandlePrivateSendOrderFailure method not found.");

        method.Invoke(realization, [order, statusCode, content, message, apiErrorPrefix, statusErrorPrefix]);
    }

    private static bool InvokeResolveCancelOrderFallback(
        OkxServerRealization realization,
        OrderStateType state,
        string failureMessage)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ResolveCancelOrderFallback",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ResolveCancelOrderFallback method not found.");

        return (bool)(method.Invoke(realization, [state, failureMessage])
            ?? throw new InvalidOperationException("ResolveCancelOrderFallback returned null."));
    }

    private static (HttpResponseMessage response, string content, ResponseRestMessage<List<ResponseWsOrders>> message) InvokeExecutePrivateOrdersQueryRequest(
        OkxServerRealization realization,
        string url)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecutePrivateOrdersQueryRequest",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ExecutePrivateOrdersQueryRequest method not found.");

        object tuple = method.Invoke(realization, [url])
            ?? throw new InvalidOperationException("ExecutePrivateOrdersQueryRequest returned null.");

        return ((HttpResponseMessage response, string content, ResponseRestMessage<List<ResponseWsOrders>> message))tuple;
    }

    private static (HttpResponseMessage response, string content, TradeDetailsResponse message) InvokeExecutePrivateTradeDetailsQueryRequest(
        OkxServerRealization realization,
        string url)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecutePrivateTradeDetailsQueryRequest",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ExecutePrivateTradeDetailsQueryRequest method not found.");

        object tuple = method.Invoke(realization, [url])
            ?? throw new InvalidOperationException("ExecutePrivateTradeDetailsQueryRequest returned null.");

        return ((HttpResponseMessage response, string content, TradeDetailsResponse message))tuple;
    }

    private static (HttpResponseMessage response, string content, string message) InvokeExecutePrivateQueryRequest(
        OkxServerRealization realization,
        string url)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecutePrivateQueryRequest",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ExecutePrivateQueryRequest method not found.");

        MethodInfo genericMethod = method.MakeGenericMethod(typeof(string));
        Func<string, string> parser = static content => content.ToUpperInvariant();

        object tuple = genericMethod.Invoke(realization, [url, parser])
            ?? throw new InvalidOperationException("ExecutePrivateQueryRequest returned null.");

        return ((HttpResponseMessage response, string content, string message))tuple;
    }

    private static HttpClient? GetPrivateHttpClientField(OkxServerRealization realization)
    {
        FieldInfo field = typeof(OkxServerRealization).GetField(
            "_privateHttpClient",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("_privateHttpClient field not found.");

        return field.GetValue(realization) as HttpClient;
    }

    private static void SetPrivateField(OkxServerRealization realization, string fieldName, object? value)
    {
        FieldInfo field = typeof(OkxServerRealization).GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"{fieldName} field not found.");

        field.SetValue(realization, value);
    }

    private static OkxServerRealization CreateRealizationForHttpClientTests()
    {
        OkxServerRealization realization = (OkxServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(OkxServerRealization));

        SetPrivateField(realization, "_privateHttpClientLocker", new Lock());
        SetPrivateField(realization, "_publicKey", "api");
        SetPrivateField(realization, "_secretKey", "secret");
        SetPrivateField(realization, "_password", "pass");
        SetPrivateField(realization, "_demoMode", false);
        SetPrivateField(realization, "_myProxy", null);

        return realization;
    }

    private sealed class ProbeHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        public HttpResponseMessage? ResponseToReturn { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(ResponseToReturn ?? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        }
    }
}
