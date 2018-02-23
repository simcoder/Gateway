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
    [Route("api/[controller]")]
    public class CompaniesController : BaseController<CompaniesController>
    {
        private readonly ICompanyService _companyService;
        public CompaniesController(ICompanyService company, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _companyService = company;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CompanyPostDto company)
        {
            var result = await _companyService.CreateCompanyAsync(company);
            return Ok(result.Value);
        }
    }
}
