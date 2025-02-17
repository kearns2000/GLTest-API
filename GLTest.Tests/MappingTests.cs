using FluentAssertions;
using GLTest.Application.Dtos;
using GLTest.Application.ExtensionMethods;
using GLTest.Domain.Entities;

namespace GLTest.Tests
{
    [TestFixture]
    public class MappingExtensionsTests
    {
        [Test]
        public void ToDto_MapsCompanyToCompanyDto()
        {
            // Arrange
            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                Exchange = "NASDAQ",
                Ticker = "TST",
                Isin = "US1234567890",
                Website = "https://test.com",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow.AddDays(1),
                UpdatedBy = "Admin"
            };

            // Act
            var dto = company.ToDto();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(company.Id);
            dto.Name.Should().Be(company.Name);
            dto.Exchange.Should().Be(company.Exchange);
            dto.Ticker.Should().Be(company.Ticker);
            dto.Isin.Should().Be(company.Isin);
            dto.Website.Should().Be(company.Website);
            dto.DateCreated.Should().Be(company.DateCreated);
            dto.DateUpdated.Should().Be(company.DateUpdated);
            dto.UpdatedBy.Should().Be(company.UpdatedBy);
        }

        [Test]
        public void ToEntity_MapsCreateCompanyDtoToCompany()
        {
            // Arrange
            var dto = new CreateCompanyDto
            {
                Name = "New Company",
                Exchange = "NYSE",
                Ticker = "NEW",
                Isin = "US9876543210",
                Website = "https://newcompany.com"
            };

            // Act
            var entity = dto.ToEntity();

            // Assert
            entity.Should().NotBeNull();
            entity.Id.Should().NotBeEmpty();
            entity.Name.Should().Be(dto.Name);
            entity.Exchange.Should().Be(dto.Exchange);
            entity.Ticker.Should().Be(dto.Ticker);
            entity.Isin.Should().Be(dto.Isin);
            entity.Website.Should().Be(dto.Website);
            entity.DateCreated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1)); 
        }

        [Test]
        public void ToEntity_MapsUpdateCompanyDtoToCompany()
        {
            // Arrange
            var dto = new UpdateCompanyDto
            {
                Id = Guid.NewGuid(),
                Name = "Updated Company",
                Exchange = "LSE",
                Ticker = "UPD",
                Isin = "GB1234567890",
                Website = "https://updated.com",
                UpdatedBy = "User123"
            };

            // Act
            var entity = dto.ToEntity();

            // Assert
            entity.Should().NotBeNull();
            entity.Id.Should().Be(dto.Id);
            entity.Name.Should().Be(dto.Name);
            entity.Exchange.Should().Be(dto.Exchange);
            entity.Ticker.Should().Be(dto.Ticker);
            entity.Isin.Should().Be(dto.Isin);
            entity.Website.Should().Be(dto.Website);
            entity.UpdatedBy.Should().Be(dto.UpdatedBy);
        }

        [Test]
        public void ToDtoList_MapsListOfCompaniesToListOfCompanyDtos()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { Id = Guid.NewGuid(), Name = "Company1", Exchange = "NYSE", Ticker = "CMP1", Isin = "US1111111111", Website = "https://company1.com", DateCreated = DateTime.UtcNow },
                new Company { Id = Guid.NewGuid(), Name = "Company2", Exchange = "NASDAQ", Ticker = "CMP2", Isin = "US2222222222", Website = "https://company2.com", DateCreated = DateTime.UtcNow }
            };

            // Act
            var dtoList = companies.ToDtoList();

            // Assert
            dtoList.Should().NotBeNull();
            dtoList.Should().HaveCount(companies.Count);
            dtoList[0].Id.Should().Be(companies[0].Id);
            dtoList[0].Name.Should().Be(companies[0].Name);
            dtoList[1].Id.Should().Be(companies[1].Id);
            dtoList[1].Name.Should().Be(companies[1].Name);
        }
    }
}
