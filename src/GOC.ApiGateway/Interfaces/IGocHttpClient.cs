using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GOC.ApiGateway
{
    public interface IGocHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string serviceName, ApiResource resource, string relativeUri);
        Task<HttpResponseMessage> PostAsync(string serviceName, ApiResource resource, string relativeUri, HttpContent content);
        Task<HttpResponseMessage> PutAsync(string serviceName, ApiResource resource, string relativeUri, HttpContent content);
        Task<HttpResponseMessage> DeleteAsync(string serviceName, ApiResource resource, Uri relativeUri);
    }
}
