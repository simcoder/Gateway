using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GOC.ApiGateway.Dtos;
using GOC.ApiGateway.Interfaces;

namespace GOC.ApiGateway.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<Result<CompanyDto>> CreateCompanyAsync(CompanyPostDto company)
        {
            var result = await _companyRepository.CreateCompanyAsync(company);

            var mappedResult = new CompanyDto
            {
                Id = result.Value.Id,
                Name = result.Value.Name,
                PhoneNumber = result.Value.PhoneNumber,
                Address = result.Value.Address
            };
            return Result.Ok(mappedResult);
        }

    }
}
