#nullable enable
#pragma warning disable CS9216

using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        }
    }
}
