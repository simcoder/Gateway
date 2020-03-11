using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GOC.ApiGateway.Dtos;
using GOC.ApiGateway.Enums;
using GOC.ApiGateway.Extensions;
using GOC.ApiGateway.Helpers;
using GOC.ApiGateway.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;

namespace GOC.ApiGateway.Repositories
{
    public class CompanyRepository : RepositoryBase<CompanyRepository>, ICompanyRepository
    {
        private ServiceNameTypes _serviceName = ServiceNameTypes.CrmService;

        private readonly ApiResource _apiTargetResource = Startup.AppSettings.Identity.Resources.Single(x => x.ResourceFriendlyName == "Crm.API");

        private readonly Policy _crmRetryPolicy;

        private const string BaseUrl = "/api/companies";

        public CompanyRepository(RetryPolicies retryPolicies, IGocHttpClient httpClient, ILoggerFactory loggerFactory) : base(httpClient, loggerFactory)
        {
            _crmRetryPolicy = retryPolicies.CrmServiceCircuitBreaker;
        }

        public async Task<Result<CompanyDto>> CreateCompanyAsync(CompanyPostDto company)
        {
            var response = await _crmRetryPolicy.ExecuteAsync(() =>
                                                              HttpClient.PostAsync(ServiceNameTypes.CrmService.ToString(), _apiTargetResource, BaseUrl, new JsonContent(company)));

            if (response.IsSuccessStatusCode)
            {
                var projectsServiceResponse = await response.ReadAsAsync<CompanyDto>();
                return Result.Ok(projectsServiceResponse);
            }

            //await HttpResponseExtensionMethods.ThrowIfClientErrorAsync(response);
            //await HttpResponseExtensionMethods.LogHttpResponseAsync(response, Logger);

            var errorMessage = await response.Content.ReadAsStringAsync();
            return Result.Fail<CompanyDto>(errorMessage);
        }
    }
}
