#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using OsEngine.Entity;
using OsEngine.Entity.WebSocketOsEngine;
using OsEngine.Market.Servers.Entity;
using OsEngine.Market.Servers.OKX.Entity;
using Xunit;

namespace OsEngine.Tests;

public class SecurityRefactorTests
{
    private sealed class CaptureHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class CaptureTraceListener : TraceListener
    {
        private readonly List<string> _messages = new List<string>();

        public IReadOnlyList<string> Messages => _messages;

        public override void Write(string? message)
        {
            if (message != null)
            {
                _messages.Add(message);
            }
        }

        public override void WriteLine(string? message)
        {
            if (message != null)
            {
                _messages.Add(message);
            }
        }
    }

    [Fact]
    public void CredentialProtector_ProtectAndTryUnprotect_ShouldRoundTrip()
    {
        string plain = "secret_value_123";

        string stored = CredentialProtector.Protect(plain);
        bool decrypted = CredentialProtector.TryUnprotect(stored, out string restored);

        Assert.True(decrypted);
        Assert.Equal(plain, restored);
    }

    [Fact]
    public void ServerParameterPassword_LoadLegacyPlainText_ShouldSetMigrationFlag()
    {
        ServerParameterPassword param = new ServerParameterPassword();

        param.LoadFromStr("Password^ApiSecret^legacy_plain_secret");

        Assert.Equal("ApiSecret", param.Name);
        Assert.Equal("legacy_plain_secret", param.Value);
        Assert.True(param.NeedMigrationSave);

        string saved = param.GetStringToSave();
        Assert.StartsWith("Password^ApiSecret^" + CredentialProtector.Prefix, saved, StringComparison.Ordinal);
    }

    [Fact]
    public void ServerParameterPassword_LoadEncryptedValue_ShouldNotRequireMigration()
    {
        ServerParameterPassword source = new ServerParameterPassword
        {
            Name = "ApiSecret",
            Value = "encrypted_secret"
        };

        string saved = source.GetStringToSave();

        ServerParameterPassword loaded = new ServerParameterPassword();
        loaded.LoadFromStr(saved);

        Assert.Equal("ApiSecret", loaded.Name);
        Assert.Equal("encrypted_secret", loaded.Value);
        Assert.False(loaded.NeedMigrationSave);
    }

    [Fact]
    public void WebSocket_IgnoreSslErrors_Property_ShouldBeInternal_AndMarkedObsolete()
    {
        PropertyInfo? property = typeof(WebSocket).GetProperty(
            "IgnoreSslErrors",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(property);
        PropertyInfo nonNullProperty = property!;

        Assert.False(nonNullProperty.GetMethod?.IsPublic ?? true);
        Assert.True(nonNullProperty.GetMethod?.IsAssembly ?? false);

        ObsoleteAttribute? attribute = nonNullProperty.GetCustomAttribute<ObsoleteAttribute>();
        Assert.NotNull(attribute);
        Assert.Contains("security risk", attribute!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WebSocket_IgnoreSslErrors_SetTrue_ShouldEmitTraceWarning()
    {
        WebSocket socket = new WebSocket("wss://example.com");
        CaptureTraceListener listener = new CaptureTraceListener();
        PropertyInfo? property = typeof(WebSocket).GetProperty(
            "IgnoreSslErrors",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(property);

        Trace.Listeners.Add(listener);

        try
        {
            property!.SetValue(socket, true);
        }
        finally
        {
            Trace.Listeners.Remove(listener);
            listener.Dispose();
            socket.Dispose();
        }

        Assert.Contains(listener.Messages, m => m.Contains("IgnoreSslErrors=true", StringComparison.Ordinal));
    }

    [Fact]
    public void OkxHttpInterceptor_ShouldConfigureSocketsHandler_WithPooledLifetime()
    {
        HttpInterceptor interceptor = new HttpInterceptor("api", "secret", "pass", demoMode: false, myProxy: null);

        SocketsHttpHandler? handler = interceptor.InnerHandler as SocketsHttpHandler;

        Assert.NotNull(handler);
        Assert.Equal(TimeSpan.FromMinutes(5), handler!.PooledConnectionLifetime);
        Assert.False(handler.UseProxy);
    }

    [Fact]
    public async Task OkxHttpInterceptor_ShouldAddSignedHeaders_AndDemoHeader()
    {
        HttpInterceptor interceptor = new HttpInterceptor("api", "secret", "pass", demoMode: true, myProxy: null);
        CaptureHandler capture = new CaptureHandler();
        interceptor.InnerHandler = capture;

        using HttpMessageInvoker invoker = new HttpMessageInvoker(interceptor);
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://www.okx.com/api/v5/trade/order");
        request.Options.Set(HttpInterceptor.SignatureBodyOptionKey, "{\"instId\":\"BTC-USDT\"}");

        HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(capture.LastRequest);

        HttpRequestMessage captured = capture.LastRequest!;
        Assert.Contains(captured.Headers.Accept, h => h.MediaType == "application/json");
        Assert.Equal("api", captured.Headers.GetValues("OK-ACCESS-KEY").Single());
        Assert.Equal("pass", captured.Headers.GetValues("OK-ACCESS-PASSPHRASE").Single());
        Assert.Equal("1", captured.Headers.GetValues("x-simulated-trading").Single());
        Assert.True(captured.Headers.GetValues("OK-ACCESS-SIGN").Single().Length > 0);
        Assert.True(captured.Headers.GetValues("OK-ACCESS-TIMESTAMP").Single().Length > 0);
    }

    [Fact]
    public void OkxHttpInterceptor_ShouldConfigureProxy_WhenProvided()
    {
        WebProxy proxy = new WebProxy("http://127.0.0.1:8888");
        HttpInterceptor interceptor = new HttpInterceptor("api", "secret", "pass", demoMode: false, myProxy: proxy);

        SocketsHttpHandler? handler = interceptor.InnerHandler as SocketsHttpHandler;

        Assert.NotNull(handler);
        Assert.True(handler!.UseProxy);
        Assert.Same(proxy, handler.Proxy);
    }

    [Fact]
    public async Task OkxHttpInterceptor_ShouldSetDemoHeaderToZero_WhenDemoModeDisabled()
    {
        HttpInterceptor interceptor = new HttpInterceptor("api", "secret", "pass", demoMode: false, myProxy: null);
        CaptureHandler capture = new CaptureHandler();
        interceptor.InnerHandler = capture;

        using HttpMessageInvoker invoker = new HttpMessageInvoker(interceptor);
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://www.okx.com/api/v5/account/balance");

        HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(capture.LastRequest);
        Assert.Equal("0", capture.LastRequest!.Headers.GetValues("x-simulated-trading").Single());
    }

    [Fact]
    public async Task OkxHttpInterceptor_ShouldEmitUtcTimestamp_InExpectedFormat()
    {
        HttpInterceptor interceptor = new HttpInterceptor("api", "secret", "pass", demoMode: false, myProxy: null);
        CaptureHandler capture = new CaptureHandler();
        interceptor.InnerHandler = capture;

        using HttpMessageInvoker invoker = new HttpMessageInvoker(interceptor);
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://www.okx.com/api/v5/public/time");

        HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(capture.LastRequest);

        string rawTimestamp = capture.LastRequest!.Headers.GetValues("OK-ACCESS-TIMESTAMP").Single();
        Assert.EndsWith("Z", rawTimestamp, StringComparison.Ordinal);

        bool parsed = DateTime.TryParseExact(
            rawTimestamp,
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out DateTime parsedTimestamp);

        Assert.True(parsed);
        Assert.Equal(DateTimeKind.Utc, parsedTimestamp.Kind);
    }

    [Fact]
    public async Task OkxHttpInterceptor_ShouldThrowInvalidOperation_WhenRequestUriIsMissing()
    {
        HttpInterceptor interceptor = new HttpInterceptor("api", "secret", "pass", demoMode: false, myProxy: null);
        CaptureHandler capture = new CaptureHandler();
        interceptor.InnerHandler = capture;

        using HttpMessageInvoker invoker = new HttpMessageInvoker(interceptor);
        using HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Get
        };

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => invoker.SendAsync(request, CancellationToken.None));

        Assert.Contains("RequestUri", error.Message, StringComparison.Ordinal);
    }
}
