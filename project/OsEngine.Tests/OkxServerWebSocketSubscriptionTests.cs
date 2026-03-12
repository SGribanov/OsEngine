#nullable enable

using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using OsEngine.Market.Servers.OKX;
using OsEngine.Market.Servers.OKX.Entity;

namespace OsEngine.Tests;

public sealed class OkxServerWebSocketSubscriptionTests
{
    [Fact]
    public void CreateSingleArgSubscribeRequestJson_WithOptionArgs_ShouldSerializeExpectedPayload()
    {
        string json = InvokeCreateSingleArgSubscribeRequestJson(new SubscribeArgsOption
        {
            channel = "opt-summary",
            instFamily = "BTC-USD",
        });

        JObject payload = JObject.Parse(json);

        Assert.Equal("subscribe", payload["op"]?.Value<string>());
        JToken arg = Assert.Single(payload["args"]!);
        Assert.Equal("opt-summary", arg["channel"]?.Value<string>());
        Assert.Equal("BTC-USD", arg["instFamily"]?.Value<string>());
    }

    [Fact]
    public void CreateSingleArgSubscribeRequestJson_WithMarkPriceArgs_ShouldSerializeExpectedPayload()
    {
        string json = InvokeCreateSingleArgSubscribeRequestJson(new SubscribeArgs
        {
            channel = "mark-price",
            instId = "BTC-USDT-SWAP",
        });

        JObject payload = JObject.Parse(json);

        Assert.Equal("subscribe", payload["op"]?.Value<string>());
        JToken arg = Assert.Single(payload["args"]!);
        Assert.Equal("mark-price", arg["channel"]?.Value<string>());
        Assert.Equal("BTC-USDT-SWAP", arg["instId"]?.Value<string>());
    }

    private static string InvokeCreateSingleArgSubscribeRequestJson<T>(T subscribeArgs)
    {
        MethodInfo methodDefinition = typeof(OkxServerRealization)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .FirstOrDefault(method => method.Name == "CreateSingleArgSubscribeRequestJson" && method.IsGenericMethodDefinition)
            ?? throw new InvalidOperationException("CreateSingleArgSubscribeRequestJson generic method not found.");

        MethodInfo method = methodDefinition.MakeGenericMethod(typeof(T));
        return (string)(method.Invoke(null, [subscribeArgs])
            ?? throw new InvalidOperationException("CreateSingleArgSubscribeRequestJson returned null."));
    }
}
