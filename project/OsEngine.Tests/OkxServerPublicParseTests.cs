#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OsEngine.Logging;
using OsEngine.Market.Servers.OKX;
using OsEngine.Market.Servers.OKX.Entity;

namespace OsEngine.Tests;

public sealed class OkxServerPublicParseTests
{
    [Fact]
    public void ExecutePublicSecurityResponseRequest_WithOkPayload_ShouldParseSecurityResponse()
    {
        const string responseBody = "{\"code\":\"0\",\"data\":[{\"instId\":\"BTC-USDT-SWAP\",\"instType\":\"SWAP\",\"tickSz\":\"0.1\"}]}";
        using LocalHttpServer server = new LocalHttpServer(HttpStatusCode.OK, responseBody);
        OkxServerRealization realization = CreateRealization(server.BaseUrl);

        string? loggedMessage = null;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        SecurityResponse response = InvokeExecutePublicSecurityResponseRequest(realization, "/probe?instType=SWAP", "Probe");

        Assert.NotNull(response);
        Assert.Equal("0", response.code);
        SecurityResponseItem item = Assert.Single(response.data);
        Assert.Equal("BTC-USDT-SWAP", item.instId);
        Assert.Equal("SWAP", item.instType);
        Assert.Equal("0.1", item.tickSz);
        Assert.Equal("/probe?instType=SWAP", server.LastRequestPath);
        Assert.Null(loggedMessage);
    }

    [Fact]
    public void ExecutePublicSecurityUnderlyingResponseRequest_WithOkPayload_ShouldParseUnderlyingResponse()
    {
        const string responseBody = "{\"code\":\"0\",\"data\":[[\"BTC-USD\",\"ETH-USD\"]]}";
        using LocalHttpServer server = new LocalHttpServer(HttpStatusCode.OK, responseBody);
        OkxServerRealization realization = CreateRealization(server.BaseUrl);

        string? loggedMessage = null;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        SecurityUnderlyingResponse response = InvokeExecutePublicSecurityUnderlyingResponseRequest(
            realization,
            "/probe?instType=OPTION",
            "Probe");

        Assert.NotNull(response);
        Assert.Equal("0", response.code);
        Assert.Single(response.data);
        Assert.Equal(["BTC-USD", "ETH-USD"], response.data[0]);
        Assert.Equal("/probe?instType=OPTION", server.LastRequestPath);
        Assert.Null(loggedMessage);
    }

    [Fact]
    public void ExecutePublicQueryRequest_ShouldReturnParsedValueFromResponseContent()
    {
        const string responseBody = "{\"probe\":\"ok\"}";
        using LocalHttpServer server = new LocalHttpServer(HttpStatusCode.OK, responseBody);
        OkxServerRealization realization = CreateRealization(server.BaseUrl);

        string? loggedMessage = null;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        string parsed = InvokeExecutePublicQueryRequest(realization, "/probe", "Probe");

        Assert.Equal("PROBE: {\"probe\":\"ok\"}", parsed);
        Assert.Equal("/probe", server.LastRequestPath);
        Assert.Null(loggedMessage);
    }

    [Fact]
    public void DeserializeAnonymousPayload_WithSecurityResponseTemplate_ShouldReturnParsedPayload()
    {
        const string responseBody = "{\"code\":\"0\",\"data\":[{\"instId\":\"BTC-USDT-SWAP\",\"instType\":\"SWAP\",\"tickSz\":\"0.1\"}]}";

        SecurityResponse response = InvokeDeserializeAnonymousPayload(responseBody, new SecurityResponse());

        Assert.Equal("0", response.code);
        SecurityResponseItem item = Assert.Single(response.data);
        Assert.Equal("BTC-USDT-SWAP", item.instId);
        Assert.Equal("SWAP", item.instType);
        Assert.Equal("0.1", item.tickSz);
    }

    [Fact]
    public void ExecuteSafePublicSecurityResponseRequest_WithOkPayload_ShouldReturnParsedResponse()
    {
        const string responseBody = "{\"code\":\"0\",\"data\":[{\"instId\":\"BTC-USDT\",\"instType\":\"SPOT\",\"tickSz\":\"0.01\"}]}";
        using LocalHttpServer server = new LocalHttpServer(HttpStatusCode.OK, responseBody);
        OkxServerRealization realization = CreateRealization(server.BaseUrl);

        string? loggedMessage = null;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        SecurityResponse? response = InvokeExecuteSafePublicSecurityResponseRequest(
            realization,
            "/probe?instType=SPOT",
            "Probe");

        Assert.NotNull(response);
        Assert.Equal("0", response.code);
        SecurityResponseItem item = Assert.Single(response.data);
        Assert.Equal("BTC-USDT", item.instId);
        Assert.Equal("SPOT", item.instType);
        Assert.Equal("/probe?instType=SPOT", server.LastRequestPath);
        Assert.Null(loggedMessage);
    }

    [Fact]
    public void ExecuteSafePublicSecurityResponseRequest_WhenInnerRequestThrows_ShouldLogAndReturnNull()
    {
        OkxServerRealization realization = CreateRealization("http://127.0.0.1");
        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        SecurityResponse? response = InvokeExecuteSafePublicSecurityResponseRequest(
            realization,
            static () => throw new InvalidOperationException("probe failure"));

        Assert.Null(response);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.NotNull(loggedMessage);
        Assert.Contains("probe failure", loggedMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void ExecuteSafePublicOperation_WithSuccessfulFactory_ShouldReturnResultWithoutLogging()
    {
        OkxServerRealization realization = CreateRealization("http://127.0.0.1");
        string? loggedMessage = null;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        string? result = InvokeExecuteSafePublicOperation(realization, static () => "probe");

        Assert.Equal("probe", result);
        Assert.Null(loggedMessage);
    }

    [Fact]
    public void ExecuteSafePublicOperation_WithThrowingFactory_ShouldLogAndReturnNull()
    {
        OkxServerRealization realization = CreateRealization("http://127.0.0.1");
        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        string? result = InvokeExecuteSafePublicOperation(
            realization,
            static () => throw new InvalidOperationException("public op failure"));

        Assert.Null(result);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.NotNull(loggedMessage);
        Assert.Contains("public op failure", loggedMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void GetOptionBaseSecurities_WithEmptyUnderlying_ShouldLogExistingMessageAndReturnNull()
    {
        const string responseBody = "{\"code\":\"0\",\"data\":[]}";
        using LocalHttpServer server = new LocalHttpServer(HttpStatusCode.OK, responseBody);
        OkxServerRealization realization = CreateRealization(server.BaseUrl);

        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        List<string>? response = InvokeGetOptionBaseSecurities(realization);

        Assert.Null(response);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.Equal("GetOptionSecurities - Empty underlying", loggedMessage);
        Assert.Equal("/api/v5/public/underlying?instType=OPTION", server.LastRequestPath);
    }

    private static OkxServerRealization CreateRealization(string baseUrl)
    {
        OkxServerRealization realization = (OkxServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(OkxServerRealization));
        SetPrivateField(realization, "_baseUrl", baseUrl);
        SetPrivateField(realization, "_myProxy", null);
        return realization;
    }

    private static SecurityResponse InvokeExecutePublicSecurityResponseRequest(
        OkxServerRealization realization,
        string resource,
        string errorLogPrefix)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecutePublicSecurityResponseRequest",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ExecutePublicSecurityResponseRequest method not found.");

        return (SecurityResponse)(method.Invoke(realization, [resource, errorLogPrefix])
            ?? throw new InvalidOperationException("ExecutePublicSecurityResponseRequest returned null."));
    }

    private static SecurityUnderlyingResponse InvokeExecutePublicSecurityUnderlyingResponseRequest(
        OkxServerRealization realization,
        string resource,
        string errorLogPrefix)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecutePublicSecurityUnderlyingResponseRequest",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ExecutePublicSecurityUnderlyingResponseRequest method not found.");

        return (SecurityUnderlyingResponse)(method.Invoke(realization, [resource, errorLogPrefix])
            ?? throw new InvalidOperationException("ExecutePublicSecurityUnderlyingResponseRequest returned null."));
    }

    private static string InvokeExecutePublicQueryRequest(
        OkxServerRealization realization,
        string resource,
        string errorLogPrefix)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecutePublicQueryRequest",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ExecutePublicQueryRequest method not found.");

        MethodInfo genericMethod = method.MakeGenericMethod(typeof(string));
        Func<string, string> parser = static content => "PROBE: " + content;

        return (string)(genericMethod.Invoke(realization, [resource, errorLogPrefix, parser])
            ?? throw new InvalidOperationException("ExecutePublicQueryRequest returned null."));
    }

    private static T InvokeDeserializeAnonymousPayload<T>(string content, T template)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "DeserializeAnonymousPayload",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("DeserializeAnonymousPayload method not found.");

        MethodInfo genericMethod = method.MakeGenericMethod(typeof(T));
        return (T)(genericMethod.Invoke(null, [content, template])
            ?? throw new InvalidOperationException("DeserializeAnonymousPayload returned null."));
    }

    private static SecurityResponse? InvokeExecuteSafePublicSecurityResponseRequest(
        OkxServerRealization realization,
        string resource,
        string errorLogPrefix)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecuteSafePublicSecurityResponseRequest",
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: [typeof(string), typeof(string)],
            modifiers: null)
            ?? throw new InvalidOperationException("ExecuteSafePublicSecurityResponseRequest method not found.");

        return (SecurityResponse?)method.Invoke(realization, [resource, errorLogPrefix]);
    }

    private static SecurityResponse? InvokeExecuteSafePublicSecurityResponseRequest(
        OkxServerRealization realization,
        Func<SecurityResponse> requestFactory)
    {
        MethodInfo methodDefinition = typeof(OkxServerRealization)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(method => method.Name == "ExecuteSafePublicOperation" && method.IsGenericMethodDefinition)
            ?? throw new InvalidOperationException("ExecuteSafePublicOperation generic method not found.");

        MethodInfo method = methodDefinition.MakeGenericMethod(typeof(SecurityResponse));
        return (SecurityResponse?)method.Invoke(realization, [requestFactory]);
    }

    private static string? InvokeExecuteSafePublicOperation(
        OkxServerRealization realization,
        Func<string> requestFactory)
    {
        MethodInfo methodDefinition = typeof(OkxServerRealization)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(method => method.Name == "ExecuteSafePublicOperation" && method.IsGenericMethodDefinition)
            ?? throw new InvalidOperationException("ExecuteSafePublicOperation generic method not found.");

        MethodInfo method = methodDefinition.MakeGenericMethod(typeof(string));
        return (string?)method.Invoke(realization, [requestFactory]);
    }

    private static List<string>? InvokeGetOptionBaseSecurities(OkxServerRealization realization)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "GetOptionBaseSecurities",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("GetOptionBaseSecurities method not found.");

        return (List<string>?)method.Invoke(realization, Array.Empty<object>());
    }

    private static void SetPrivateField(OkxServerRealization realization, string fieldName, object? value)
    {
        FieldInfo field = typeof(OkxServerRealization).GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"{fieldName} field not found.");

        field.SetValue(realization, value);
    }

    private sealed class LocalHttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Task _worker;
        private readonly string _responseBody;
        private readonly HttpStatusCode _statusCode;

        public LocalHttpServer(HttpStatusCode statusCode, string responseBody)
        {
            _statusCode = statusCode;
            _responseBody = responseBody;

            int port = GetFreePort();
            BaseUrl = $"http://127.0.0.1:{port}";
            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl + "/");
            _listener.Start();
            _worker = Task.Run(HandleSingleRequestAsync);
        }

        public string BaseUrl { get; }

        public string? LastRequestPath { get; private set; }

        public void Dispose()
        {
            _listener.Stop();
            _listener.Close();
            _worker.GetAwaiter().GetResult();
        }

        private async Task HandleSingleRequestAsync()
        {
            try
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                LastRequestPath = context.Request.RawUrl;
                context.Response.StatusCode = (int)_statusCode;
                byte[] buffer = Encoding.UTF8.GetBytes(_responseBody);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
            catch (HttpListenerException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private static int GetFreePort()
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
    }
}
