#nullable enable

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Logging;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.Finam;

namespace OsEngine.Tests;

public sealed class FinamServerHttpRequestTests
{
    [Fact]
    public void CreateFinamProbeRequestMessage_ShouldSetUserAgentWithoutMutatingSharedClientHeaders()
    {
        FinamServerRealization server = new FinamServerRealization();

        using HttpRequestMessage request = InvokeCreateFinamProbeRequestMessage();

        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://www.finam.ru/profile/moex-akcii/sberbank/export/old/", request.RequestUri?.ToString());
        Assert.True(request.Headers.TryGetValues("User-Agent", out var values));
        string userAgent = string.Join(" ", values ?? Enumerable.Empty<string>());
        Assert.Contains("Mozilla/5.0", userAgent, StringComparison.Ordinal);
        Assert.Contains("Chrome/139.0.0.0", userAgent, StringComparison.Ordinal);
        Assert.Empty(server.HttpClient.DefaultRequestHeaders);
    }

    [Fact]
    public void Connect_WhenHttpClientThrows_ShouldLogAndRemainDisconnected()
    {
        FinamServerRealization server = new FinamServerRealization
        {
            HttpClient = new HttpClient(new ThrowingHandler())
        };

        string? capturedMessage = null;
        LogMessageType? capturedType = null;
        server.LogMessageEvent += (message, type) =>
        {
            capturedMessage = message;
            capturedType = type;
        };

        server.Connect((WebProxy)null!);

        Assert.Equal(ServerConnectStatus.Disconnect, server.ServerStatus);
        Assert.Equal(LogMessageType.Error, capturedType);
        Assert.Equal("Connect server error: Simulated request failure", capturedMessage);
    }

    private static HttpRequestMessage InvokeCreateFinamProbeRequestMessage()
    {
        MethodInfo method = typeof(FinamServerRealization).GetMethod(
            "CreateFinamProbeRequestMessage",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("CreateFinamProbeRequestMessage method not found.");

        try
        {
            return (HttpRequestMessage)method.Invoke(null, null)!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Simulated request failure");
        }
    }
}
