using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace GOC.ApiGateway
{
    public class HttpClientWrapper : IGocHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly Func<string, string, Uri> _relativeUriResolver;
        private IHttpTokenAuthorizationContext _authContext { get; set; }

        public HttpClientWrapper(HttpClient client, IHttpTokenAuthorizationContext authContext,Func<string, string, Uri> serviceUriResolver = null)
        {
            _authContext = authContext;
            _httpClient = client;
            _relativeUriResolver = serviceUriResolver ?? ((sn, s) => new Uri(s));
        }

        /// <summary>
        /// Invokes the configured URI resolver on a relative URI string to
        /// return an equivalent System.Uri.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="relativeUri"></param>
        /// <returns></returns>
        public Uri ResolveRelativeUri(string serviceName, string relativeUri)
        {
            return _relativeUriResolver(serviceName, relativeUri);
        }

        public async Task<HttpResponseMessage> GetAsync(string serviceName, DownstreamClient downstreamClient, string relativeUri, CancellationToken cancellationToken)
        {
            await SetAccessToken(downstreamClient);
            var request = CreateRequestMessage(HttpMethod.Get, serviceName, relativeUri);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        HttpRequestMessage CreateRequestMessage(HttpMethod method, string serviceName, string relativeUri)
        {
            var uri = _relativeUriResolver.Invoke(serviceName, relativeUri);
            var request = new HttpRequestMessage(method, uri);
            return request;
        }

        async Task SetAccessToken(DownstreamClient downstreamClient)
        {
            var delegateToken = await DelegateAsync(_authContext.AccessToken, downstreamClient.ClientId, downstreamClient.ClientSecret, downstreamClient.ResourceName);
            _httpClient.SetBearerToken(delegateToken.AccessToken);
        }

        async Task<TokenResponse> DelegateAsync(string userToken, string clientId, string clientSecret, string resourceName)
        {
            var payload = new
            {
                token = userToken
            };

            // create token client
            var client = new TokenClient(Startup.AppSettings.Identity.TokenEndpointUrl, clientId, clientSecret);

            // send custom grant to token endpoint, return response
            return await client.RequestCustomGrantAsync("delegation", resourceName, payload);
        }

    }
}
