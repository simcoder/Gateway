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

        /// <summary>Send a GET request to the specified Uri as an asynchronous operation.</summary>
        /// <returns>
        ///     Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous
        ///     operation.
        /// </returns>
        /// <param name="serviceName">Name of consul service which is used to get address of service.</param>
        /// <param name="relativeUri">The relative Uri the request is sent to.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="relativeUri" /> was null.</exception>
        public async Task<HttpResponseMessage> GetAsync(string serviceName, ApiResource resource, string relativeUri)
        {
            return await GetAsync(serviceName, resource ,relativeUri, CancellationToken.None);
        }

        /// <summary>Send a POST request to the specified Uri as an asynchronous operation.</summary>
        /// <returns>
        ///     Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous
        ///     operation.
        /// </returns>
        /// <param name="serviceName">Name of consul service which is used to get address of service.</param>
        /// <param name="relativeUri">The relative Uri the request is sent to.</param>
        /// <param name="content">The HTTP request content sent to the server.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="relativeUri" /> was null.</exception>
        public async Task<HttpResponseMessage> PostAsync(string serviceName, ApiResource resource, string relativeUri, HttpContent content)
        {
            return await PostAsync(serviceName, resource, relativeUri, content, CancellationToken.None);
        }

        /// <summary>Send a PUT request to the specified Uri as an asynchronous operation.</summary>
        /// <returns>
        ///     Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous
        ///     operation.
        /// </returns>
        /// <param name="serviceName">Name of consul service which is used to get address of service.</param>
        /// <param name="relativeUri">The relative Uri the request is sent to.</param>
        /// <param name="content">The HTTP request content sent to the server.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="relativeUri" /> was null.</exception>
        public async Task<HttpResponseMessage> PutAsync(string serviceName, ApiResource resource, string relativeUri, HttpContent content)
        {
            return await PutAsync(serviceName, resource, relativeUri, content, CancellationToken.None);
        }

        /// <summary>Send a DELETE request to the specified Uri as an asynchronous operation.</summary>
        /// <returns>
        ///     Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous
        ///     operation.
        /// </returns>
        /// <param name="serviceName">Name of consul service which is used to get address of service.</param>
        /// <param name="relativeUri">The relative Uri the request is sent to.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="relativeUri" /> was null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///     The request message was already sent by the
        ///     <see cref="T:System.Net.Http.HttpClient" /> instance.
        /// </exception>
        public async Task<HttpResponseMessage> DeleteAsync(string serviceName, ApiResource resource, Uri relativeUri)
        {
            return await DeleteAsync(serviceName, resource, relativeUri.ToString(), CancellationToken.None);
        }

        async Task<HttpResponseMessage> PostAsync(string serviceName, ApiResource resource, string relativeUri, HttpContent content, CancellationToken cancellationToken)
        {
            await SetAccessToken(resource);
            var request = CreateRequestMessage(HttpMethod.Post, serviceName, relativeUri);
            request.Content = content;
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        async Task<HttpResponseMessage> PutAsync(string serviceName, ApiResource resource, string relativeUri, HttpContent content, CancellationToken cancellationToken)
        {
            await SetAccessToken(resource);
            var request = CreateRequestMessage(HttpMethod.Put, serviceName, relativeUri);
            request.Content = content;
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        async Task<HttpResponseMessage> DeleteAsync(string serviceName, ApiResource resource, string relativeUri, CancellationToken cancellationToken)
        {
            await SetAccessToken(resource);
            var request = CreateRequestMessage(HttpMethod.Delete, serviceName, relativeUri);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        async Task<HttpResponseMessage> GetAsync(string serviceName, ApiResource resource, string relativeUri, CancellationToken cancellationToken)
        {
            await SetAccessToken(resource);
            var request = CreateRequestMessage(HttpMethod.Get, serviceName, relativeUri);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        Uri ResolveRelativeUri(string serviceName, string relativeUri)
        {
            return _relativeUriResolver(serviceName, relativeUri);
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
            var result = await client.RequestCustomGrantAsync("delegation", resourceName, payload);
            return result;
        }

        async Task SetAccessToken(ApiResource resource)
        {
            var delegateToken = await DelegateAsync(_authContext.AccessToken, Startup.AppSettings.Identity.ApiClientId, Startup.AppSettings.Identity.ApiClientSecret, resource.ResourceName);
            _httpClient.SetBearerToken(delegateToken.AccessToken);
        }

        HttpRequestMessage CreateRequestMessage(HttpMethod method, string serviceName, string relativeUri)
        {
            var uri = _relativeUriResolver.Invoke(serviceName, relativeUri);
            var request = new HttpRequestMessage(method, uri);
            return request;
        }

    }
}
