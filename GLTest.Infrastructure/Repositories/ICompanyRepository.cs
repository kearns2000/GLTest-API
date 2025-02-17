using GLTest.Domain.Entities;

namespace GLTest.Infrastructure.Repositories
{
    public interface ICompanyRepository
    {
        Task<List<Company>> GetAllAsync( CancellationToken cancellationToken);
        Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Company?> GetByIsinAsync(string isin, CancellationToken cancellationToken);
        Task AddAsync(Company company, CancellationToken cancellationToken);
        Task UpdateAsync(Company company, CancellationToken cancellationToken);
      
    }
}
