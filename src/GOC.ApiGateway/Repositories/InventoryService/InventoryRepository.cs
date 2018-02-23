using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using CSharpFunctionalExtensions;
using GOC.ApiGateway.Extensions;
using GOC.ApiGateway.Enums;
using System.Linq;
using GOC.ApiGateway.Interfaces;
using GOC.ApiGateway.Dtos;
using GOC.ApiGateway.Helpers;

namespace GOC.ApiGateway.Repositories
{
    public class InventoryRepository : RepositoryBase<InventoryRepository>, IInventoryRepository
    {
        private ServiceNameTypes _serviceName = ServiceNameTypes.InventoryService;

        private readonly ApiResource _apiTargetResource = Startup.AppSettings.Identity.Resources.Single(x => x.ResourceFriendlyName == "Inventory.API");

        private readonly Policy _inventoryRetryPolicy;

        public InventoryRepository(RetryPolicies retryPolicies, IGocHttpClient client, ILoggerFactory loggerFactory) : base(client, loggerFactory)
        {
            _inventoryRetryPolicy = retryPolicies.InventoryServiceCircuitBreaker;

        }

        private const string BaseUrl = "/api/inventories";


        public async Task<Result<IEnumerable<InventoryDto>>> GetAllInventoriesAsync()
        {
            var response = await _inventoryRetryPolicy.ExecuteAsync(() => HttpClient.GetAsync(ServiceNameTypes.InventoryService.ToString(), _apiTargetResource, BaseUrl));

            if (response.IsSuccessStatusCode)
            {
                var projectsServiceResponse = await response.ReadAsAsync<IEnumerable<InventoryDto>>();
                return Result.Ok(projectsServiceResponse);
            }

            //await HttpResponseExtensionMethods.ThrowIfClientErrorAsync(response);
            //await HttpResponseExtensionMethods.LogHttpResponseAsync(response, Logger);

            var errorMessage = await response.Content.ReadAsStringAsync();
            return Result.Fail<IEnumerable<InventoryDto>>(errorMessage);
        }

        public async Task<Result<InventoryDto>> CreateInventoryAsync(InventoryPostDto inventory)
        {
            var response = await _inventoryRetryPolicy.ExecuteAsync(() => 
                                                                    HttpClient.PostAsync(ServiceNameTypes.InventoryService.ToString(), _apiTargetResource, BaseUrl, new JsonContent(inventory)));

            if (response.IsSuccessStatusCode)
            {
                var projectsServiceResponse = await response.ReadAsAsync<InventoryDto>();
                return Result.Ok(projectsServiceResponse);
            }

            //await HttpResponseExtensionMethods.ThrowIfClientErrorAsync(response);
            //await HttpResponseExtensionMethods.LogHttpResponseAsync(response, Logger);

            var errorMessage = await response.Content.ReadAsStringAsync();
            return Result.Fail<InventoryDto>(errorMessage);
        }
    }
}
