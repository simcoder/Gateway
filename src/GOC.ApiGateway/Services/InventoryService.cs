using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GOC.ApiGateway.Dtos;
using GOC.ApiGateway.Interfaces;

namespace GOC.ApiGateway.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<Result<InventoryDto>> CreateInventoryAsync(InventoryPostDto inventory)
        {
            var result = await _inventoryRepository.CreateInventoryAsync(inventory);

            var mappedResult = new InventoryDto
            {
                Id = result.Value.Id,
                CompanyId = result.Value.CompanyId,
                Name = result.Value.Name
            };
            return Result.Ok(mappedResult);
        }
    }
}
