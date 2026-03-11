#nullable enable

using System;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Market.Servers.OKX;
using RestSharp;

namespace OsEngine.Tests;

public sealed class OkxServerPublicRestClientTests
{
    [Fact]
    public void CreatePublicRestClient_WithoutProxy_ShouldUseBaseUrlAndLeaveProxyUnset()
    {
        OkxServerRealization realization = CreateRealization();

        RestClient client = InvokeCreatePublicRestClient(realization);

        Assert.Equal("https://www.okx.com/", client.BaseUrl?.AbsoluteUri);
        Assert.Null(client.Proxy);
    }

    [Fact]
    public void CreatePublicRestClient_WithProxy_ShouldUseConfiguredProxy()
    {
        OkxServerRealization realization = CreateRealization();
        WebProxy proxy = new WebProxy("http://127.0.0.1:8888");
        SetPrivateField(realization, "_myProxy", proxy);

        RestClient client = InvokeCreatePublicRestClient(realization);

        Assert.Equal("https://www.okx.com/", client.BaseUrl?.AbsoluteUri);
        Assert.Same(proxy, client.Proxy);
    }

    private static OkxServerRealization CreateRealization()
    {
        OkxServerRealization realization = (OkxServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(OkxServerRealization));
        SetPrivateField(realization, "_baseUrl", "https://www.okx.com");
        SetPrivateField(realization, "_myProxy", null);
        return realization;
    }

    private static RestClient InvokeCreatePublicRestClient(OkxServerRealization realization)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "CreatePublicRestClient",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("CreatePublicRestClient method not found.");

        return (RestClient)(method.Invoke(realization, null)
            ?? throw new InvalidOperationException("CreatePublicRestClient returned null."));
    }

    private static void SetPrivateField(OkxServerRealization realization, string fieldName, object? value)
    {
        FieldInfo field = typeof(OkxServerRealization).GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"{fieldName} field not found.");

        field.SetValue(realization, value);
    }
}
