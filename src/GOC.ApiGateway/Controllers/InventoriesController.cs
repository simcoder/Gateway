using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GOC.ApiGateway.Dtos;
using GOC.ApiGateway.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GOC.ApiGateway.Controllers
{
    [Authorize]
    [Route("api/companies/{companyId}/[controller]")]
    public class InventoriesController : BaseController<InventoriesController>
    {
        private  IInventoryService _inventoryService;
        public InventoriesController(IInventoryService inventoryService, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return View();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Post(Guid companyId, InventoryPostDto inventory)
        {
            inventory.CompanyId = companyId;
            var result = await _inventoryService.CreateInventoryAsync(inventory);
            return Ok(result.Value);
        }
    }
}
