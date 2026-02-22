#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using OsEngine.Market.Servers.Entity;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace OsEngine.Market.Servers.OKX.Entity
{
    public class HttpInterceptor : DelegatingHandler
    {
        private readonly string _apiKey;
        private readonly string _passPhrase;
        private readonly string _secret;
        private readonly bool _demoMode;
        public static readonly HttpRequestOptionsKey<string> SignatureBodyOptionKey = new HttpRequestOptionsKey<string>("okx-signature-body");

        //Задерждка для рест запросов
        public RateGate _rateGateRest = new RateGate(1, TimeSpan.FromMilliseconds(200));

        public HttpInterceptor(string apiKey, string secret, string passPhrase, bool demoMode, WebProxy myProxy)
        {
            this._apiKey = apiKey;
            this._passPhrase = passPhrase;
            this._secret = secret;
            this._demoMode = demoMode;

            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                UseProxy = myProxy != null,
                Proxy = myProxy
            };

            InnerHandler = socketsHandler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _rateGateRest.WaitToProceed();

            var method = request.Method.Method;
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("OK-ACCESS-KEY", this._apiKey);

            if (request.RequestUri == null)
            {
                throw new InvalidOperationException("RequestUri must be set for OKX signed request.");
            }

            var timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            var requestUrl = request.RequestUri.PathAndQuery;
            request.Options.TryGetValue(SignatureBodyOptionKey, out string bodyStr);
            string sign;
            if (!String.IsNullOrEmpty(bodyStr))
            {
                sign = Encryptor.HmacSHA256($"{timeStamp}{method}{requestUrl}{bodyStr}", this._secret);
            }
            else
            {
                sign = Encryptor.HmacSHA256($"{timeStamp}{method}{requestUrl}", this._secret);
            }

            request.Headers.Add("OK-ACCESS-SIGN", sign);
            request.Headers.Add("OK-ACCESS-TIMESTAMP", timeStamp.ToString());
            request.Headers.Add("OK-ACCESS-PASSPHRASE", this._passPhrase);

            if (_demoMode)
            {
                request.Headers.Add("x-simulated-trading", "1");
            }
            else
            {
                request.Headers.Add("x-simulated-trading", "0");
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}

