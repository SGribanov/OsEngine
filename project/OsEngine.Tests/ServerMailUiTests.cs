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
        string[] addresses = InvokeStatic<string[]>("ParseAddresses", " first@example.com \r\n\r\nsecond@example.com\n   \nthird@example.com ");

        Assert.Equal(["first@example.com", "second@example.com", "third@example.com"], addresses);
    }

    [Fact]
    public void ParseAddresses_ShouldReturnNullWhenNoRecipientsRemain()
    {
        string[]? addresses = InvokeStatic<string[]?>("ParseAddresses", "\r\n\r\n");

        Assert.Null(addresses);
    }

    [Theory]
    [InlineData("smtp.yandex.ru", "Yandex")]
    [InlineData("SMTP.YANDEX.RU", "Yandex")]
    [InlineData("smtp.gmail.com", "Google")]
    [InlineData(null, "Google")]
    public void GetProviderNameForSmtp_ShouldMapHostsToProvider(string? smtpHost, string expectedProvider)
    {
        string provider = InvokeStatic<string>("GetProviderNameForSmtp", smtpHost);

        Assert.Equal(expectedProvider, provider);
    }

    [Theory]
    [InlineData("Yandex", "smtp.yandex.ru")]
    [InlineData("Google", "smtp.gmail.com")]
    [InlineData(null, "smtp.gmail.com")]
    [InlineData("Unknown", "smtp.gmail.com")]
    public void ResolveSmtpHost_ShouldPreserveFallbackBehavior(string? selection, string expectedHost)
    {
        string host = InvokeStatic<string>("ResolveSmtpHost", selection);

        Assert.Equal(expectedHost, host);
    }

    private static T InvokeStatic<T>(string methodName, params object?[]? args)
    {
        MethodInfo method = typeof(ServerMailDeliveryUi).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method not found: " + methodName);

        object? result = method.Invoke(null, args);

        return (T)result!;
    }
}
