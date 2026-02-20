using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Market.Servers.OKX.Entity;
using Xunit;

namespace OsEngine.Tests;

public class OkxHttpInterceptorTests
{
    [Fact]
    public async Task SendAsync_WithSignatureBodyOption_ShouldSignBodyAndSetDemoHeader()
    {
        string apiKey = "key";
        string secret = "secret";
        string passPhrase = "pass";
        string body = "{\"instId\":\"BTC-USDT\"}";

        var interceptor = new HttpInterceptor(apiKey, secret, passPhrase, demoMode: true, myProxy: null!);
        var probe = new ProbeHandler();
        interceptor.InnerHandler = probe;

        using HttpMessageInvoker invoker = new HttpMessageInvoker(interceptor);
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://www.okx.com/api/v5/trade/order");
        request.Options.Set(HttpInterceptor.SignatureBodyOptionKey, body);
        request.Content = new StringContent(body);

        using HttpResponseMessage _ = await invoker.SendAsync(request, CancellationToken.None);

        HttpRequestMessage sent = probe.LastRequest!;
        Assert.NotNull(sent);
        Assert.Equal("application/json", sent.Headers.Accept.First().MediaType);
        Assert.Equal("1", sent.Headers.GetValues("x-simulated-trading").Single());
        Assert.Equal(apiKey, sent.Headers.GetValues("OK-ACCESS-KEY").Single());
        Assert.Equal(passPhrase, sent.Headers.GetValues("OK-ACCESS-PASSPHRASE").Single());
        Assert.NotNull(sent.RequestUri);

        string ts = sent.Headers.GetValues("OK-ACCESS-TIMESTAMP").Single();
        string actualSign = sent.Headers.GetValues("OK-ACCESS-SIGN").Single();
        string expectedPayload = $"{ts}{HttpMethod.Post.Method}{sent.RequestUri!.PathAndQuery}{body}";
        string expectedSign = ComputeHmacSha256Base64(expectedPayload, secret);

        Assert.Equal(expectedSign, actualSign);
    }

    [Fact]
    public async Task SendAsync_WithoutSignatureBodyOption_ShouldSignWithoutBodyAndSetLiveHeader()
    {
        string apiKey = "key2";
        string secret = "secret2";
        string passPhrase = "pass2";

        var interceptor = new HttpInterceptor(apiKey, secret, passPhrase, demoMode: false, myProxy: null!);
        var probe = new ProbeHandler();
        interceptor.InnerHandler = probe;

        using HttpMessageInvoker invoker = new HttpMessageInvoker(interceptor);
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://www.okx.com/api/v5/account/balance");

        using HttpResponseMessage _ = await invoker.SendAsync(request, CancellationToken.None);

        HttpRequestMessage sent = probe.LastRequest!;
        Assert.NotNull(sent);
        Assert.Equal("0", sent.Headers.GetValues("x-simulated-trading").Single());
        Assert.NotNull(sent.RequestUri);

        string ts = sent.Headers.GetValues("OK-ACCESS-TIMESTAMP").Single();
        string actualSign = sent.Headers.GetValues("OK-ACCESS-SIGN").Single();
        string expectedPayload = $"{ts}{HttpMethod.Get.Method}{sent.RequestUri!.PathAndQuery}";
        string expectedSign = ComputeHmacSha256Base64(expectedPayload, secret);

        Assert.Equal(expectedSign, actualSign);
    }

    private sealed class ProbeHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        }
    }

    private static string ComputeHmacSha256Base64(string payload, string secret)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
        byte[] dataBytes = Encoding.UTF8.GetBytes(payload);

        using HMACSHA256 hmac = new HMACSHA256(keyBytes);
        byte[] hash = hmac.ComputeHash(dataBytes);
        return System.Convert.ToBase64String(hash);
    }
}
