﻿using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GOC.ApiGateway.Dtos;

namespace GOC.ApiGateway.Interfaces
{
    public interface IInventoryService
    {
        Task<Result<InventoryDto>> CreateInventoryAsync(InventoryPostDto inventory);
    }
}
