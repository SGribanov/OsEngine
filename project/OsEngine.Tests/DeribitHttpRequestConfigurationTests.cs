#nullable enable

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using OsEngine.Market.Servers.Deribit;

namespace OsEngine.Tests;

public sealed class DeribitHttpRequestConfigurationTests
{
    private sealed class CaptureHandler : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string? AuthorizationScheme { get; private set; }

        public string? AuthorizationParameter { get; private set; }

        public string? ContentType { get; private set; }

        public string? Body { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Body = request.Content?.ReadAsStringAsync().Result;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        }
    }

    [Fact]
    public void CreatePrivateQuery_ShouldSignRequestMessageWithoutMutatingSharedClientHeaders()
    {
        DeribitServerRealization server = new DeribitServerRealization();
        CaptureHandler handler = new CaptureHandler();
        HttpClient client = new HttpClient(handler);

        SetField(server, "_baseUrl", "https://www.deribit.com");
        SetField(server, "_clientID", "client-id");
        SetField(server, "_secretKey", "secret-key");
        SetField(server, "_httpClient", client);

        HttpResponseMessage response = InvokeCreatePrivateQuery(
            server,
            "/api/v2/private/buy",
            "POST",
            "{\"instrument_name\":\"BTC-PERPETUAL\"}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://www.deribit.com/api/v2/private/buy", handler.RequestUri?.ToString());
        Assert.Equal("deri-hmac-sha256", handler.AuthorizationScheme);
        Assert.NotNull(handler.AuthorizationParameter);
        Assert.Contains("id=client-id", handler.AuthorizationParameter!, StringComparison.Ordinal);
        Assert.Contains(",ts=", handler.AuthorizationParameter!, StringComparison.Ordinal);
        Assert.Contains(",sig=", handler.AuthorizationParameter!, StringComparison.Ordinal);
        Assert.Contains(",nonce=abcd", handler.AuthorizationParameter!, StringComparison.Ordinal);
        Assert.Equal("application/json", handler.ContentType);
        Assert.Equal("{\"instrument_name\":\"BTC-PERPETUAL\"}", handler.Body);
        Assert.Empty(client.DefaultRequestHeaders);
    }

    private static HttpResponseMessage InvokeCreatePrivateQuery(
        DeribitServerRealization server,
        string requestPath,
        string method,
        string? requestBody)
    {
        MethodInfo createPrivateQueryMethod = typeof(DeribitServerRealization).GetMethod(
            "CreatePrivateQuery",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("CreatePrivateQuery method not found.");

        try
        {
            return (HttpResponseMessage)createPrivateQueryMethod.Invoke(server, [requestPath, method, requestBody])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    private static void SetField(DeribitServerRealization server, string fieldName, object value)
    {
        FieldInfo field = typeof(DeribitServerRealization).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"{fieldName} field not found.");

        field.SetValue(server, value);
    }
}
