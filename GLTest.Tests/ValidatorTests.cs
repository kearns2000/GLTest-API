using FluentValidation.TestHelper;
using GLTest.Application.Dtos;
using GLTest.Application.Validation;

namespace GLTest.Tests 
{
    [TestFixture]
    public class CreateCompanyValidatorTests
    {
        private CreateCompanyValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new CreateCompanyValidator();
        }

        [Test]
        public void ValidCompanyDto_PassesValidation()
        {
            // Arrange
            var dto = new CreateCompanyDto
            {
                Name = "Valid Company",
                Exchange = "NYSE",
                Ticker = "AAPL",
                Isin = "US1234567890",
                Website = "https://www.example.com"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }      

        [Test]
        public void TickerExceedsMaxLength_FailsValidation()
        {
            // Arrange
            var dto = new CreateCompanyDto { Ticker = "TOO_LONG_TICKER" };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Ticker);
        }

        [Test]
        public void InvalidIsin_FailsValidation()
        {
            // Arrange
            var dto = new CreateCompanyDto { Isin = "1234567890US" };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Isin);
        }

        [Test]
        public void ValidWebsiteUrl_PassesValidation()
        {
            // Arrange
            var dto = new CreateCompanyDto { Website = "https://www.valid-url.com" };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Website);
        }

        [Test]
        public void InvalidWebsiteUrl_FailsValidation()
        {
            // Arrange
            var dto = new CreateCompanyDto { Website = "invalid-url" };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Website);
        }

        [Test]
        public void EmptyWebsite_PassesValidation()
        {
            // Arrange
            var dto = new CreateCompanyDto { Website = "" };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Website);
        }
    }
}
