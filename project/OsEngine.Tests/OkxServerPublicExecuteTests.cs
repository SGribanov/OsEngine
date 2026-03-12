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
using RestSharp;

namespace OsEngine.Tests;

public sealed class OkxServerPublicExecuteTests
{
    [Fact]
    public void ExecutePublicGetRequest_WithOkStatus_ShouldReturnResponseWithoutLogging()
    {
        using LocalHttpServer server = new LocalHttpServer(HttpStatusCode.OK, "{\"data\":[]}");
        OkxServerRealization realization = CreateRealization(server.BaseUrl);

        string? loggedMessage = null;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        IRestResponse response = InvokeExecutePublicGetRequest(realization, "/probe", "Probe");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"data\":[]}", response.Content);
        Assert.Null(loggedMessage);
        Assert.Equal("/probe", server.LastRequestPath);
    }

    [Fact]
    public void ExecutePublicGetRequest_WithNonOkStatus_ShouldLogPrefixAndResponseContent()
    {
        using LocalHttpServer server = new LocalHttpServer(HttpStatusCode.BadGateway, "gateway down");
        OkxServerRealization realization = CreateRealization(server.BaseUrl);

        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        IRestResponse response = InvokeExecutePublicGetRequest(realization, "/probe", "Probe");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal("gateway down", response.Content);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.Equal("Probe - gateway down", loggedMessage);
        Assert.Equal("/probe", server.LastRequestPath);
    }

    [Fact]
    public void HasOkPublicAbsoluteStatus_WithOkStatus_ShouldReturnTrueWithoutLogging()
    {
        OkxServerRealization realization = CreateRealization("http://127.0.0.1");
        RestResponse response = new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = "{\"data\":[]}",
        };

        string? loggedMessage = null;
        realization.LogMessageEvent += (message, _) => loggedMessage = message;

        bool result = InvokeHasOkPublicAbsoluteStatus(realization, response, static currentResponse => $"Probe - {currentResponse.Content}");

        Assert.True(result);
        Assert.Null(loggedMessage);
    }

    [Fact]
    public void HasOkPublicAbsoluteStatus_WithNonOkStatus_ShouldLogFactoryMessageAndReturnFalse()
    {
        OkxServerRealization realization = CreateRealization("http://127.0.0.1");
        RestResponse response = new RestResponse
        {
            StatusCode = HttpStatusCode.BadGateway,
            Content = "gateway down",
        };

        string? loggedMessage = null;
        LogMessageType? loggedType = null;
        realization.LogMessageEvent += (message, type) =>
        {
            loggedMessage = message;
            loggedType = type;
        };

        bool result = InvokeHasOkPublicAbsoluteStatus(
            realization,
            response,
            static currentResponse => $"Probe - {currentResponse.StatusCode} - {currentResponse.Content}");

        Assert.False(result);
        Assert.Equal(LogMessageType.Error, loggedType);
        Assert.Equal("Probe - BadGateway - gateway down", loggedMessage);
    }

    private static OkxServerRealization CreateRealization(string baseUrl)
    {
        OkxServerRealization realization = (OkxServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(OkxServerRealization));
        SetPrivateField(realization, "_baseUrl", baseUrl);
        SetPrivateField(realization, "_myProxy", null);
        return realization;
    }

    private static IRestResponse InvokeExecutePublicGetRequest(OkxServerRealization realization, string resource, string errorLogPrefix)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecutePublicGetRequest",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ExecutePublicGetRequest method not found.");

        return (IRestResponse)(method.Invoke(realization, [resource, errorLogPrefix])
            ?? throw new InvalidOperationException("ExecutePublicGetRequest returned null."));
    }

    private static bool InvokeHasOkPublicAbsoluteStatus(
        OkxServerRealization realization,
        IRestResponse response,
        Func<IRestResponse, string> errorMessageFactory)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "HasOkPublicAbsoluteStatus",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("HasOkPublicAbsoluteStatus method not found.");

        return (bool)(method.Invoke(realization, [response, errorMessageFactory])
            ?? throw new InvalidOperationException("HasOkPublicAbsoluteStatus returned null."));
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
