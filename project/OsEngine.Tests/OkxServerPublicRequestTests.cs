#nullable enable

using System;
using System.Reflection;
using OsEngine.Market.Servers.OKX;
using RestSharp;

namespace OsEngine.Tests;

public sealed class OkxServerPublicRequestTests
{
    [Fact]
    public void CreatePublicGetRequest_ShouldPreserveResourceAndMethod()
    {
        const string resource = "/api/v5/public/instruments?instType=SWAP";

        RestRequest request = InvokeCreatePublicGetRequest(resource);

        Assert.Equal(Method.GET, request.Method);
        Assert.Equal("/api/v5/public/instruments", request.Resource);
        Parameter queryParameter = Assert.Single(request.Parameters);
        Assert.Equal("instType", queryParameter.Name);
        Assert.Equal("SWAP", queryParameter.Value);
    }

    private static RestRequest InvokeCreatePublicGetRequest(string resource)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "CreatePublicGetRequest",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("CreatePublicGetRequest method not found.");

        return (RestRequest)(method.Invoke(null, [resource])
            ?? throw new InvalidOperationException("CreatePublicGetRequest returned null."));
    }
}
