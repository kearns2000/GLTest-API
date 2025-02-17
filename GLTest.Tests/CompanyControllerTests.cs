using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using GLTest.API.Controllers;
using GLTest.Application.Common;
using GLTest.Application.Dtos;
using GLTest.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;


namespace GLTest.Tests
{
    [TestFixture]
    public class CompanyControllerTests
    {
        private Mock<ICompanyService> _mockCompanyService;
        private Mock<IValidator<CreateCompanyDto>> _mockCreateValidator;
        private Mock<IValidator<UpdateCompanyDto>> _mockUpdateValidator;
        private CompanyController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockCompanyService = new Mock<ICompanyService>();
            _mockCreateValidator = new Mock<IValidator<CreateCompanyDto>>();
            _mockUpdateValidator = new Mock<IValidator<UpdateCompanyDto>>();

            _controller = new CompanyController(
                _mockCompanyService.Object,
                _mockCreateValidator.Object,
                _mockUpdateValidator.Object
            );
        }

        [Test]
        public async Task GetAll_WhenCalled_ReturnsListOfCompanies()
        {
            // Arrange
            var companies = new List<CompanyDto>
    {
        new CompanyDto { Id = Guid.NewGuid(), Name = "Company A" },
        new CompanyDto { Id = Guid.NewGuid(), Name = "Company B" }
    };

            _mockCompanyService
                .Setup(s => s.GetAllCompanies(It.IsAny<CancellationToken>()))
                .ReturnsAsync(companies);

            // Act
            var actionResult = await _controller.GetAll(CancellationToken.None);

            // Assert
            actionResult.Should().NotBeNull();
                      
            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
                      
            var response = okResult.Value as Result<List<CompanyDto>>;
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
        }

        [Test]
        public async Task GetById_WhenCompanyExists_ReturnsCompany()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var company = new CompanyDto { Id = companyId, Name = "Company A" };

            _mockCompanyService
                .Setup(s => s.GetCompanyById(companyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(company);

            // Act
            var actionResult = await _controller.GetById(companyId, CancellationToken.None);

            // Assert
            actionResult.Should().NotBeNull();
                     
            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
                  
            var response = okResult.Value as Result<CompanyDto>;
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data.Id.Should().Be(companyId);
        }


        [Test]
        public async Task GetById_WhenCompanyDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            _mockCompanyService
                .Setup(s => s.GetCompanyById(companyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CompanyDto)null);

            // Act
            var actionResult = await _controller.GetById(companyId, CancellationToken.None);

            // Assert
            actionResult.Should().NotBeNull();
                      
            var notFoundResult = actionResult.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
                    
            var response = notFoundResult.Value as Result<CompanyDto>;
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("Company not found");
        }


        [Test]
        public async Task Create_WhenValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var companyDto = new CreateCompanyDto { Name = "New Company" };
            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Company name is required")
            };

            _mockCreateValidator
                .Setup(v => v.ValidateAsync(companyDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationErrors));

            // Act
            var result = await _controller.Create(companyDto, CancellationToken.None) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var problemDetails = result.Value as ProblemDetails;
            problemDetails.Should().NotBeNull();
            problemDetails!.Title.Should().Be("Validation failed");
        }

        [Test]
        public async Task Create_WhenBusinessValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var companyDto = new CreateCompanyDto { Name = "New Company" };
            var serviceResult = Result<CompanyDto>.FailureResult("Company already exists");

            _mockCreateValidator
                .Setup(v => v.ValidateAsync(companyDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockCompanyService
                .Setup(s => s.CreateCompany(companyDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Create(companyDto, CancellationToken.None) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        public async Task Create_WhenSuccessful_ReturnsCreatedAtAction()
        {
            // Arrange
            var companyDto = new CreateCompanyDto { Name = "New Company" };
            var createdCompany = new CompanyDto { Id = Guid.NewGuid(), Name = "New Company" };
            var serviceResult = Result<CompanyDto>.SuccessResult(createdCompany, "Company created successfully.");

            _mockCreateValidator
                .Setup(v => v.ValidateAsync(companyDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockCompanyService
                .Setup(s => s.CreateCompany(companyDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Create(companyDto, CancellationToken.None) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status201Created);
        }

        [Test]
        public async Task Update_WhenValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var companyDto = new UpdateCompanyDto { Id = Guid.NewGuid(), Name = "Updated Company" };
            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Company name is required")
            };

            _mockUpdateValidator
                .Setup(v => v.ValidateAsync(companyDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationErrors));

            // Act
            var result = await _controller.Update(companyDto.Id, companyDto, CancellationToken.None) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

     
    }
}
