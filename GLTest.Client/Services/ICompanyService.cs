using GLTest.Client.Models;

namespace GLTest.Client.Services
{
    public interface ICompanyService
    {
        Task<Result<List<Company>>> GetCompaniesAsync();
        Task<Result<Company>> GetCompanyAsync(Guid id);
        Task<Result<Company>> GetCompanyByIsinAsync(string isin);
        Task<Result<bool>> CreateCompanyAsync(Company model);
        Task<Result<bool>> UpdateCompanyAsync(Company model);

    }
}

