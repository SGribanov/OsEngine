#nullable enable
using System;
using System.Reflection;
using OsEngine.Logging;

namespace OsEngine.Tests;

public class ServerMailUiTests
{
    [Fact]
    public void ParseAddresses_ShouldPreserveNonEmptyRecipientsAcrossCrLf()
    {
        string[] addresses = InvokeStatic<string[]>("ParseAddresses", "first@example.com\r\n\r\nsecond@example.com\n");

        Assert.Equal(["first@example.com", "second@example.com"], addresses);
    }

    [Fact]
    public void ParseAddresses_ShouldReturnNullWhenNoRecipientsRemain()
    {
        string[]? addresses = InvokeStatic<string[]?>("ParseAddresses", "\r\n\r\n");

        Assert.Null(addresses);
    }

    [Fact]
    public void ResolveSmtpHost_ShouldDefaultToGoogleForNullSelection()
    {
        string host = InvokeStatic<string>("ResolveSmtpHost", new object?[] { null });

        Assert.Equal("smtp.gmail.com", host);
    }

    [Fact]
    public void ResolveSmtpHost_ShouldReturnYandexHostForYandexSelection()
    {
        string host = InvokeStatic<string>("ResolveSmtpHost", "Yandex");

        Assert.Equal("smtp.yandex.ru", host);
    }

    private static T InvokeStatic<T>(string methodName, params object?[]? args)
    {
        MethodInfo method = typeof(ServerMailDeliveryUi).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method not found: " + methodName);

        object? result = method.Invoke(null, args);

        return (T)result!;
    }
}
