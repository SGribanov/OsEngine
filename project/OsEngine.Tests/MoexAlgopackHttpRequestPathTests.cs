#nullable enable

using System;
using System.Net.Http;
using OsEngine.Market.Servers.MoexAlgopack;
using OsEngine.Market.Servers.MoexAlgopack.Entity;

namespace OsEngine.Tests;

public sealed class MoexAlgopackHttpRequestPathTests
{
    [Fact]
    public void CreateAuthRequestMessage_ShouldSetBasicAuthorizationOnRequest()
    {
        using HttpClient client = new HttpClient();
        using HttpRequestMessage request = MoexAlgopackAuth.CreateAuthRequestMessage(
            "https://passport.moex.com/authenticate",
            "user",
            "pass");

        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://passport.moex.com/authenticate", request.RequestUri?.ToString());
        Assert.Equal("Basic", request.Headers.Authorization?.Scheme);
        Assert.Equal("dXNlcjpwYXNz", request.Headers.Authorization?.Parameter);
        Assert.Empty(client.DefaultRequestHeaders);
    }

    [Fact]
    public void CreatePublicRequestUri_RelativePath_ShouldCombineWithBaseUrl()
    {
        Uri requestUri = MoexAlgopackServer.MoexAlgopackServerRealization.CreatePublicRequestUri(
            "https://iss.moex.com/iss",
            "/engines/stock/markets/shares/boards/tqbr/securities.json?iss.meta=off&iss.only=securities");

        Assert.Equal(
            "https://iss.moex.com/iss/engines/stock/markets/shares/boards/tqbr/securities.json?iss.meta=off&iss.only=securities",
            requestUri.AbsoluteUri);
    }

    [Fact]
    public void CreatePublicRequestUri_AbsoluteUrl_ShouldPreserveAbsoluteRequest()
    {
        Uri requestUri = MoexAlgopackServer.MoexAlgopackServerRealization.CreatePublicRequestUri(
            "https://iss.moex.com/iss",
            "https://iss.moex.com/iss/engines/futures/markets/forts/securities.json?iss.meta=off&iss.only=securities");

        Assert.Equal(
            "https://iss.moex.com/iss/engines/futures/markets/forts/securities.json?iss.meta=off&iss.only=securities",
            requestUri.AbsoluteUri);
    }
}
