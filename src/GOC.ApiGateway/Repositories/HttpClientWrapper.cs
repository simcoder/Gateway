using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GOC.ApiGateway
{
    public class HttpClientWrapper : IGocHttpBasicClient
    {
        private readonly HttpClient _httpClient;
        private readonly Func<string, string, Uri> _relativeUriResolver;
        private readonly Action<HttpRequestMessage> _httpRequestMessageDecorator;

        public HttpClientWrapper(HttpClient client, Func<string, string, Uri> serviceUriResolver = null, Action<HttpRequestMessage> httpRequestMessageDecorator = null)
        {
            _httpClient = client;
            _relativeUriResolver = serviceUriResolver ?? ((sn, s) => new Uri(s));
            _httpRequestMessageDecorator = httpRequestMessageDecorator ?? (m => { });
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

        public async Task<HttpResponseMessage> GetAsync(string serviceName, string relativeUri, CancellationToken cancellationToken)
        {
            var request = CreateRequestMessage(HttpMethod.Get, serviceName, relativeUri);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string serviceName, string relativeUri)
        {
            var uri = _relativeUriResolver.Invoke(serviceName, relativeUri);
            var request = new HttpRequestMessage(method, uri);
            _httpRequestMessageDecorator.Invoke(request);
            return request;
        }

    }
}
