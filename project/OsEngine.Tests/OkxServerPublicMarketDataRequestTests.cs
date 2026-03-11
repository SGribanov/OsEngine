#nullable enable

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OsEngine.Market.Servers.OKX;
using RestSharp;

namespace OsEngine.Tests;

public sealed class OkxServerPublicMarketDataRequestTests
{
    [Fact]
    public void ExecutePublicAbsoluteGetRequest_WithOkStatus_ShouldPreserveUrlAndResponse()
    {
        using LocalHttpServer server = new LocalHttpServer(HttpStatusCode.OK, "{\"data\":[]}");

        IRestResponse response = InvokeExecutePublicAbsoluteGetRequest($"{server.BaseUrl}/api/v5/market/candles?instId=BTC-USDT-SWAP&limit=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"data\":[]}", response.Content);
        Assert.Equal("/api/v5/market/candles?instId=BTC-USDT-SWAP&limit=100", server.LastRequestPath);
    }

    [Fact]
    public void ExecutePublicAbsoluteGetRequest_WithNonOkStatus_ShouldReturnResponseUntouched()
    {
        using LocalHttpServer server = new LocalHttpServer(HttpStatusCode.BadGateway, "gateway down");

        IRestResponse response = InvokeExecutePublicAbsoluteGetRequest($"{server.BaseUrl}/api/v5/public/funding-rate-history?instId=BTC-USDT-SWAP");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal("gateway down", response.Content);
        Assert.Equal("/api/v5/public/funding-rate-history?instId=BTC-USDT-SWAP", server.LastRequestPath);
    }

    private static IRestResponse InvokeExecutePublicAbsoluteGetRequest(string url)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "ExecutePublicAbsoluteGetRequest",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ExecutePublicAbsoluteGetRequest method not found.");

        return (IRestResponse)(method.Invoke(null, [url])
            ?? throw new InvalidOperationException("ExecutePublicAbsoluteGetRequest returned null."));
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
