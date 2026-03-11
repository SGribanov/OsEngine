#nullable enable

using System;
using System.IO;
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
