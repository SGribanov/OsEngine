#nullable enable

using System;
using OsEngine.Market.Servers.Polygon;

namespace OsEngine.Tests;

public sealed class PolygonHttpRequestPathTests
{
    [Fact]
    public void CreateRequestUri_RelativePath_ShouldCombineBaseUrlAndAppendApiKey()
    {
        Uri requestUri = PolygonServerRealization.CreateRequestUri(
            "https://api.massive.com",
            "/v3/reference/tickers?active=true&limit=10",
            "secret key");

        Assert.Equal(
            "https://api.massive.com/v3/reference/tickers?active=true&limit=10&apiKey=secret%20key",
            requestUri.AbsoluteUri);
    }

    [Fact]
    public void CreateRequestUri_AbsoluteNextUrlWithoutApiKey_ShouldAppendApiKeyOnce()
    {
        Uri requestUri = PolygonServerRealization.CreateRequestUri(
            "https://api.massive.com",
            "https://api.massive.com/v3/reference/tickers?cursor=abc",
            "secret");

        Assert.Equal(
            "https://api.massive.com/v3/reference/tickers?cursor=abc&apiKey=secret",
            requestUri.AbsoluteUri);
    }

    [Fact]
    public void CreateRequestUri_AbsoluteNextUrlWithApiKey_ShouldPreserveExistingQuery()
    {
        Uri requestUri = PolygonServerRealization.CreateRequestUri(
            "https://api.massive.com",
            "https://api.massive.com/v3/reference/tickers?cursor=abc&apiKey=existing",
            "secret");

        Assert.Equal(
            "https://api.massive.com/v3/reference/tickers?cursor=abc&apiKey=existing",
            requestUri.AbsoluteUri);
    }
}
