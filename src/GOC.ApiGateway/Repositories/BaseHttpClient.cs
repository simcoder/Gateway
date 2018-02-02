using System.Net.Http;

namespace GOC.ApiGateway
{
    public class BaseHttpClient 
    {
        private readonly HttpClient _httpClient;

        public BaseHttpClient(HttpClient client)
        {
            _httpClient = client;
        }

    }
}
