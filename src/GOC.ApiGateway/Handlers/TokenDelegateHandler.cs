using System;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace GOC.ApiGateway.Handlers
{
    public class TokenDelegateHandler
    {
        public async Task<TokenResponse> DelegateAsync(string userToken, string authority, DownstreamClient downstreamClient)
        {
            var payload = new
            {
                token = userToken
            };

            // create token client
            var client = new TokenClient($"{authority}/connect/token", downstreamClient.ApiName, downstreamClient.ApiSecret);


            return await client.RequestCustomGrantAsync("delegation", "api2", payload);
        }
    }
}
