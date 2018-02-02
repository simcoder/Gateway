using System.Collections.Generic;

namespace GOC.ApiGateway
{
    public interface IHttpTokenAuthorizationContext
    {
        string AccessToken { get; }

        IEnumerable<string> BearerTokens { get; }
    }
}
