using GLTest.Application.Common;
using GLTest.Application.Dtos;

namespace GLTest.Application.Services
{
    public interface ICompanyService
    {
        Task<List<CompanyDto>> GetAllCompanies(CancellationToken cancellationToken);
        Task<CompanyDto?> GetCompanyById(Guid id, CancellationToken cancellationToken);
        Task<CompanyDto?> GetCompanyByIsin(string isin, CancellationToken cancellationToken);
        Task<Result<CompanyDto>> CreateCompany(CreateCompanyDto createCompanyDto, CancellationToken cancellationToken);
        Task<Result<CompanyDto>> UpdateCompany(UpdateCompanyDto updateCompanyDto, CancellationToken cancellationToken);
  
    }
}
