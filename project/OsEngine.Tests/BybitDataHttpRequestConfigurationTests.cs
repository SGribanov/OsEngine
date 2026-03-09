#nullable enable

using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using OsEngine.Market.Servers.BybitData;

namespace OsEngine.Tests;

public sealed class BybitDataHttpRequestConfigurationTests
{
    [Fact]
    public void CreateConfiguredRequestMessage_ShouldBuildStringRequestWithoutMutatingSharedClientHeaders()
    {
        BybitDataServerRealization server = new BybitDataServerRealization();

        using HttpRequestMessage request = InvokeCreateConfiguredRequestMessage(
            server,
            "https://public.bybit.com/trading/BTCUSDT/",
            "StringContent");

        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://public.bybit.com/trading/BTCUSDT/", request.RequestUri?.ToString());
        AssertHeaderValue(request, "Accept", "*/*");
        AssertHeaderValue(request, "Referer", "https://www.bybit.com/derivatives/en/history-data");
        AssertHeaderValue(request, "X-Kl-Saas-Ajax-Request", "Ajax_Request");

        HttpClient client = GetHttpClient(server);
        Assert.Empty(client.DefaultRequestHeaders);
    }

    [Fact]
    public void CreateConfiguredRequestMessage_ShouldBuildFileDownloadRequestWithExpectedNavigationHeaders()
    {
        BybitDataServerRealization server = new BybitDataServerRealization();

        using HttpRequestMessage request = InvokeCreateConfiguredRequestMessage(
            server,
            "https://public.bybit.com/trading/BTCUSDT/BTCUSDT2024-01-01.csv.gz",
            "FileDownload");

        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://public.bybit.com/trading/BTCUSDT/BTCUSDT2024-01-01.csv.gz", request.RequestUri?.ToString());
        AssertHeaderValue(request, "Cache-control", "max-age=0");
        AssertHeaderValue(request, "Sec-Fetch-Dest", "document");
        AssertHeaderValue(request, "Sec-Fetch-Mode", "navigate");
        AssertHeaderValue(request, "Upgrade-insecure-requests", "1");
    }

    private static HttpRequestMessage InvokeCreateConfiguredRequestMessage(
        BybitDataServerRealization server,
        string requestUri,
        string profileName)
    {
        MethodInfo method = typeof(BybitDataServerRealization).GetMethod(
            "CreateConfiguredRequestMessage",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("CreateConfiguredRequestMessage method not found.");

        Type profileType = typeof(BybitDataServerRealization).GetNestedType(
            "BybitRequestProfile",
            BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("BybitRequestProfile enum not found.");

        object profile = Enum.Parse(profileType, profileName);

        try
        {
            return (HttpRequestMessage)method.Invoke(server, [requestUri, profile])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    private static HttpClient GetHttpClient(BybitDataServerRealization server)
    {
        FieldInfo field = typeof(BybitDataServerRealization).GetField(
            "_httpClient",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("_httpClient field not found.");

        return (HttpClient)field.GetValue(server)!;
    }

    private static void AssertHeaderValue(HttpRequestMessage request, string headerName, string expectedValue)
    {
        Assert.True(
            request.Headers.TryGetValues(headerName, out var values),
            $"Expected header '{headerName}' to be present.");
        Assert.Contains(expectedValue, values ?? Enumerable.Empty<string>());
    }
}
