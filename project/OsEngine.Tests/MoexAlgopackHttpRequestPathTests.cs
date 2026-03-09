#nullable enable

using System;
using OsEngine.Market.Servers.MoexAlgopack;

namespace OsEngine.Tests;

public sealed class MoexAlgopackHttpRequestPathTests
{
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
