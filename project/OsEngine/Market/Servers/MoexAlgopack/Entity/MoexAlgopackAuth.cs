#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace OsEngine.Market.Servers.MoexAlgopack.Entity
{
    public class MoexAlgopackAuth
    {
        private readonly string _urlAuth = "https://passport.moex.com/authenticate";
        private readonly string _urlUri = "https://www.moex.com";
        private readonly string _username;
        private readonly string _password;

        public HttpStatusCode LastStatus { get; private set; }
        public string LastStatusText { get; private set; } = string.Empty;
        public Cookie Passport { get; private set; } = new Cookie();

        public MoexAlgopackAuth(string user_name, string passwd)
        {
            _username = user_name;
            _password = passwd;
            Auth();
        }

        public void Auth()
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();

                handler.UseCookies = true;
                handler.CookieContainer = new CookieContainer();

                using HttpClient httpClient = new HttpClient(handler);
                using HttpRequestMessage request = CreateAuthRequestMessage(_urlAuth, _username, _password);
                using HttpResponseMessage response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                LastStatus = response.StatusCode;

                CookieCollection cookies = handler.CookieContainer.GetCookies(new Uri(_urlUri));
                Cookie passportCookie = null;

                for (int i = 0; i < cookies.Count; i++)
                {
                    Cookie cook = cookies[i];

                    if (cook.Name == "MicexPassportCert")
                    {
                        passportCookie = cook;
                        break;
                    }
                }

                Passport = passportCookie ?? new Cookie();
                LastStatusText = passportCookie != null ? "OK" : "Passport cookie not found";
            }
            catch (HttpRequestException e) when (e.StatusCode != null)
            {
                LastStatus = e.StatusCode.Value;
                LastStatusText = e.Message;
            }
            catch (Exception e)
            {
                LastStatus = HttpStatusCode.BadRequest;
                LastStatusText = e.Message;
            }
        }

        internal static HttpRequestMessage CreateAuthRequestMessage(string requestUrl, string username, string password)
        {
            byte[] binData = System.Text.Encoding.UTF8.GetBytes(username + ":" + password);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(binData));
            return request;
        }

        public bool IsRealTime()
        {
            if (Passport == null || (Passport != null && Passport.Expired))
            {
                Auth();
            }

            if (Passport != null && !Passport.Expired && Passport.Name == "MicexPassportCert")
            {
                return true;
            }

            return false;
        }
    }
}

