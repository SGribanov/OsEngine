#nullable enable

using System;
using System.Collections.Concurrent;
using System.Reflection;
using OsEngine.Market.Servers.OKX;

namespace OsEngine.Tests;

public sealed class OkxServerWebSocketRoutingTests
{
    [Theory]
    [InlineData("BTC-USDT-SWAP", "swap")]
    [InlineData("BTC-USD-240329-50000-C", "option")]
    [InlineData("BTC-USD-240329", "futures")]
    [InlineData("BTC-USDT", "spot")]
    public void EnqueuePublicInstrumentMessage_ShouldRouteMessageToExpectedQueue(string instId, string expectedQueue)
    {
        const string message = "{\"probe\":true}";

        ConcurrentQueue<string> spotQueue = new ConcurrentQueue<string>();
        ConcurrentQueue<string> swapQueue = new ConcurrentQueue<string>();
        ConcurrentQueue<string> futuresQueue = new ConcurrentQueue<string>();
        ConcurrentQueue<string> optionQueue = new ConcurrentQueue<string>();

        InvokeEnqueuePublicInstrumentMessage(instId, message, spotQueue, swapQueue, futuresQueue, optionQueue);

        AssertQueueState(spotQueue, expectedQueue == "spot" ? message : null);
        AssertQueueState(swapQueue, expectedQueue == "swap" ? message : null);
        AssertQueueState(futuresQueue, expectedQueue == "futures" ? message : null);
        AssertQueueState(optionQueue, expectedQueue == "option" ? message : null);
    }

    private static void InvokeEnqueuePublicInstrumentMessage(
        string instId,
        string message,
        ConcurrentQueue<string> spotQueue,
        ConcurrentQueue<string> swapQueue,
        ConcurrentQueue<string> futuresQueue,
        ConcurrentQueue<string> optionQueue)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "EnqueuePublicInstrumentMessage",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("EnqueuePublicInstrumentMessage method not found.");

        method.Invoke(null, [instId, message, spotQueue, swapQueue, futuresQueue, optionQueue]);
    }

    private static void AssertQueueState(ConcurrentQueue<string> queue, string? expectedMessage)
    {
        if (expectedMessage == null)
        {
            Assert.Empty(queue);
            return;
        }

        string routedMessage = Assert.Single(queue);
        Assert.Equal(expectedMessage, routedMessage);
    }
}
