using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GOC.ApiGateway.HttpClientHelper
{
    public class HttpTokenAuthorizationContext : IHttpTokenAuthorizationContext
    {
        private readonly Func<HttpContext> _httpContextAccessor;
        private readonly Func<HttpContext, Task<IEnumerable<string>>> _bearerTokenAccessor;
        private readonly Func<HttpContext, Task<string>> _accessTokenAccessor;

        public HttpTokenAuthorizationContext(Func<HttpContext> httpContextAccessor,
            Func<HttpContext, Task<IEnumerable<string>>> bearerTokenAccessor,
            Func<HttpContext, Task<string>> accessTokenAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _bearerTokenAccessor = bearerTokenAccessor;
            _accessTokenAccessor = accessTokenAccessor;
        }

        public string AccessToken => _accessTokenAccessor(_httpContextAccessor()).Result;

        public IEnumerable<string> BearerTokens => _bearerTokenAccessor(_httpContextAccessor()).Result;
    }
}
