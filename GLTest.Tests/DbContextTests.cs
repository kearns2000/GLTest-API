using FluentAssertions;
using GLTest.Domain.Entities;
using GLTest.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GLTest.Tests
{
    [TestFixture]
    public class ApplicationDbContextTests
    {
        private ApplicationDbContext _dbContext;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose(); 
        }

        [Test]
        public async Task AddCompany_ShouldPersistToDatabase()
        {
            // Arrange
            var newCompany = new Company { Id = Guid.NewGuid(), Name = "Test Corp", Isin = "US1234567890", DateCreated = DateTime.UtcNow };

            // Act
            _dbContext.Companies.Add(newCompany);
            await _dbContext.SaveChangesAsync();

            // Assert
            var company = await _dbContext.Companies.FindAsync(newCompany.Id);
            company.Should().NotBeNull();
            company!.Name.Should().Be("Test Corp");
        }      

        [Test]
        public async Task UpdateCompany_ShouldModifyCompany()
        {
            // Arrange
            var company = new Company { Id = Guid.NewGuid(), Name = "Old Name", Isin = "US3333333333", DateCreated = DateTime.UtcNow };
            _dbContext.Companies.Add(company);
            await _dbContext.SaveChangesAsync();

            // Act
            company.Name = "New Name";
            _dbContext.Companies.Update(company);
            await _dbContext.SaveChangesAsync();

            // Assert
            var updatedCompany = await _dbContext.Companies.FindAsync(company.Id);
            updatedCompany!.Name.Should().Be("New Name");
        }

        [Test]
        public async Task DeleteCompany_ShouldRemoveCompany()
        {
            // Arrange
            var company = new Company { Id = Guid.NewGuid(), Name = "ToDelete", Isin = "US4444444444", DateCreated = DateTime.UtcNow };
            _dbContext.Companies.Add(company);
            await _dbContext.SaveChangesAsync();

            // Act
            _dbContext.Companies.Remove(company);
            await _dbContext.SaveChangesAsync();

            // Assert
            var deletedCompany = await _dbContext.Companies.FindAsync(company.Id);
            deletedCompany.Should().BeNull();
        }
    }
}
