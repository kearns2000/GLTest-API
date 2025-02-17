using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using GLTest.Application.Dtos;
using GLTest.Application.Services;
using GLTest.Domain.Entities;
using GLTest.Infrastructure.Repositories;
using Moq;

namespace GLTest.Tests
{
    [TestFixture]
    public class CompanyServiceTests
    {
        private Mock<ICompanyRepository> _mockRepository;
        private Mock<IValidator<CreateCompanyDto>> _mockCreateValidator;
        private Mock<IValidator<UpdateCompanyDto>> _mockUpdateValidator;
        private CompanyService _service;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<ICompanyRepository>();
            _mockCreateValidator = new Mock<IValidator<CreateCompanyDto>>();
            _mockUpdateValidator = new Mock<IValidator<UpdateCompanyDto>>();

            _service = new CompanyService(_mockRepository.Object, _mockUpdateValidator.Object, _mockCreateValidator.Object);
        }

        [Test]
        public async Task GetAllCompanies_ReturnsMappedDtos()
        {
            // Arrange
            var companies = new List<Company>
            {
                new() { Id = Guid.NewGuid(), Name = "Company1", Exchange = "NYSE", Ticker = "CMP1", Isin = "US123", Website = "https://company1.com", DateCreated = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "Company2", Exchange = "NASDAQ", Ticker = "CMP2", Isin = "US456", Website = "https://company2.com", DateCreated = DateTime.UtcNow }
            };

            _mockRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(companies);

            // Act
            var result = await _service.GetAllCompanies(CancellationToken.None);

            // Assert
            result.Should().NotBeNull().And.HaveCount(2);
            result[0].Name.Should().Be("Company1");
            result[1].Name.Should().Be("Company2");
        }

        [Test]
        public async Task GetCompanyById_ExistingId_ReturnsMappedDto()
        {
            // Arrange
            var company = new Company { Id = Guid.NewGuid(), Name = "Test Company", Isin = "US1234567890" };
            _mockRepository.Setup(repo => repo.GetByIdAsync(company.Id, It.IsAny<CancellationToken>())).ReturnsAsync(company);

            // Act
            var result = await _service.GetCompanyById(company.Id, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Company");
        }

        [Test]
        public async Task GetCompanyById_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Company)null);

            // Act
            var result = await _service.GetCompanyById(Guid.NewGuid(), CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetCompanyByIsin_ExistingIsin_ReturnsMappedDto()
        {
            // Arrange
            var company = new Company { Id = Guid.NewGuid(), Name = "Company X", Isin = "US9876543210" };
            _mockRepository.Setup(repo => repo.GetByIsinAsync(company.Isin, It.IsAny<CancellationToken>())).ReturnsAsync(company);

            // Act
            var result = await _service.GetCompanyByIsin(company.Isin, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Company X");
        }

        [Test]
        public async Task GetCompanyByIsin_NonExistingIsin_ReturnsNull()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetByIsinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Company)null);

            // Act
            var result = await _service.GetCompanyByIsin("NonExistentISIN", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task CreateCompany_ValidDto_ReturnsSuccessResult()
        {
            // Arrange
            var createDto = new CreateCompanyDto { Name = "Valid Company", Isin = "US123" };
            _mockCreateValidator.Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new ValidationResult());

            _mockRepository.Setup(repo => repo.GetByIsinAsync(createDto.Isin, It.IsAny<CancellationToken>())).ReturnsAsync((Company)null);

            // Act
            var result = await _service.CreateCompany(createDto, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Company created successfully.");
        }    

        [Test]
        public async Task CreateCompany_InvalidDto_ReturnsValidationErrors()
        {
            // Arrange
            var createDto = new CreateCompanyDto { Name = "", Isin = "" };
            var validationFailures = new List<ValidationFailure>
            {
                new("Name", "Name is required"),
                new("Isin", "ISIN is required")
            };

            _mockCreateValidator.Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var result = await _service.CreateCompany(createDto, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().ContainKeys("Name", "Isin");
        }
    }
}
