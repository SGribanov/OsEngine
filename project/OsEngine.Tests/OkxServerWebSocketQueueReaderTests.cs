#nullable enable

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Market.Servers.OKX;

namespace OsEngine.Tests;

public sealed class OkxServerWebSocketQueueReaderTests
{
    [Fact]
    public void RunQueueMessageReader_WithQueuedMessage_ShouldDequeueAndInvokeHandler()
    {
        OkxServerRealization realization = CreateRealization();
        ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
        messageQueue.Enqueue("probe");

        string? processedMessage = null;

        InvokeRunQueueMessageReader(
            realization,
            messageQueue,
            message =>
            {
                processedMessage = message;
                realization.IsCompletelyDeleted = true;
            });

        Assert.Equal("probe", processedMessage);
        Assert.Empty(messageQueue);
    }

    [Fact]
    public void RunQueueMessageReader_WithDeletedFlagAndEmptyQueue_ShouldReturnWithoutInvokingHandler()
    {
        OkxServerRealization realization = CreateRealization();
        realization.IsCompletelyDeleted = true;
        ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

        bool wasInvoked = false;

        InvokeRunQueueMessageReader(
            realization,
            messageQueue,
            _ => wasInvoked = true);

        Assert.False(wasInvoked);
        Assert.Empty(messageQueue);
    }

    private static OkxServerRealization CreateRealization()
    {
        return (OkxServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(OkxServerRealization));
    }

    private static void InvokeRunQueueMessageReader(
        OkxServerRealization realization,
        ConcurrentQueue<string> messageQueue,
        Action<string> messageHandler)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "RunQueueMessageReader",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("RunQueueMessageReader method not found.");

        method.Invoke(realization, [messageQueue, messageHandler]);
    }
}
