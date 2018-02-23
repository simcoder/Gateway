using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GOC.ApiGateway.Dtos;

namespace GOC.ApiGateway.Interfaces
{
    public interface ICompanyRepository
    {
        Task<Result<CompanyDto>> CreateCompanyAsync(CompanyPostDto company);
    }
}
