#nullable enable

using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using OsEngine.Market.Servers.TraderNet;

namespace OsEngine.Tests;

public sealed class TraderNetServerHttpRequestTests
{
    [Fact]
    public async Task CreateAuthRequestMessage_ShouldSetSignatureOnRequestWithoutMutatingSharedClientHeaders()
    {
        TraderNetServerRealization server = new TraderNetServerRealization();

        using HttpRequestMessage request = InvokeCreateAuthRequestMessage(
            "https://tradernet.ru/api/v2/cmd/getSidInfo",
            "POST",
            "apiKey=test&cmd=getSidInfo",
            "test-signature");

        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://tradernet.ru/api/v2/cmd/getSidInfo", request.RequestUri?.ToString());
        Assert.True(request.Headers.TryGetValues("X-NtApi-Sig", out var signatureValues));
        Assert.Contains("test-signature", signatureValues ?? Enumerable.Empty<string>());
        Assert.Equal("apiKey=test&cmd=getSidInfo", await request.Content!.ReadAsStringAsync());
        Assert.Equal("application/x-www-form-urlencoded", request.Content.Headers.ContentType?.MediaType);
        Assert.Empty(GetHttpClient(server).DefaultRequestHeaders);
    }

    [Fact]
    public async Task CreateRequestMessage_ShouldBuildPostRequestWithoutMutatingSharedClientHeaders()
    {
        TraderNetServerRealization server = new TraderNetServerRealization();

        using HttpRequestMessage request = InvokeCreateRequestMessage(
            "https://tradernet.ru/api/",
            "POST",
            "{\"q\":{\"cmd\":\"getPositionJson\"}}",
            "application/json");

        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://tradernet.ru/api/", request.RequestUri?.ToString());
        Assert.Equal("{\"q\":{\"cmd\":\"getPositionJson\"}}", await request.Content!.ReadAsStringAsync());
        Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
        Assert.Empty(GetHttpClient(server).DefaultRequestHeaders);
    }

    private static HttpRequestMessage InvokeCreateAuthRequestMessage(string url, string method, string payload, string signature)
    {
        MethodInfo methodInfo = typeof(TraderNetServerRealization).GetMethod(
            "CreateAuthRequestMessage",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("CreateAuthRequestMessage method not found.");

        try
        {
            return (HttpRequestMessage)methodInfo.Invoke(null, [url, method, payload, signature])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    private static HttpRequestMessage InvokeCreateRequestMessage(string url, string method, string payload, string contentType)
    {
        MethodInfo methodInfo = typeof(TraderNetServerRealization).GetMethod(
            "CreateRequestMessage",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("CreateRequestMessage method not found.");

        try
        {
            return (HttpRequestMessage)methodInfo.Invoke(null, [url, method, payload, contentType])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    private static HttpClient GetHttpClient(TraderNetServerRealization server)
    {
        FieldInfo field = typeof(TraderNetServerRealization).GetField(
            "_httpClient",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("_httpClient field not found.");

        return (HttpClient)field.GetValue(server)!;
    }
}
