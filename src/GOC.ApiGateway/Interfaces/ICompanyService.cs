using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GOC.ApiGateway.Dtos;

namespace GOC.ApiGateway.Interfaces
{
    public interface ICompanyService
    {
        Task<Result<CompanyDto>> CreateCompanyAsync(CompanyPostDto company);
    }
}
