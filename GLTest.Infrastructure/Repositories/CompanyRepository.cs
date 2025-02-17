using GLTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GLTest.Infrastructure.Persistence;

namespace GLTest.Infrastructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAllAsync(CancellationToken cancellationToken) 
        { 
          return  await _context.Companies.ToListAsync(cancellationToken); 
        }

        public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {             
            return await _context.Companies.FindAsync(id, cancellationToken);        
        }

        public async Task<Company?> GetByIsinAsync(string isin, CancellationToken cancellationToken)
        {
            return await _context.Companies.FirstOrDefaultAsync(c => c.Isin == isin, cancellationToken);
        }

        public async Task AddAsync(Company company, CancellationToken cancellationToken)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync(cancellationToken);

        }

        public async Task UpdateAsync(Company company, CancellationToken cancellationToken)
        {
            _context.Companies.Update(company);
            await _context.SaveChangesAsync(cancellationToken);
        }


    }
}
