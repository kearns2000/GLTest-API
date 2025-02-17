using System.IdentityModel.Tokens.Jwt;
using System.Text;
using GLTest.API.Controllers;
using GLTest.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using FluentAssertions;
using NUnit.Framework;
using Castle.Core.Configuration;
using System.Security.Claims;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using GLTest.Tests.Models;
using System.Text.Json;

namespace GLTest.Tests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private AuthController _authController;
        private Mock<IConfiguration> _mockConfiguration;

        [SetUp]
        public void Setup()
        {
            var mockConfig = new Dictionary<string, string>
{
    { "JwtSettings:Secret", "SuperSecretKey12345678901234567890" },
    { "JwtSettings:Issuer", "TestIssuer" },
    { "JwtSettings:Audience", "TestAudience" }
};

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c.GetSection("JwtSettings")["Secret"]).Returns(mockConfig["JwtSettings:Secret"]);
            _mockConfiguration.Setup(c => c.GetSection("JwtSettings")["Issuer"]).Returns(mockConfig["JwtSettings:Issuer"]);
            _mockConfiguration.Setup(c => c.GetSection("JwtSettings")["Audience"]).Returns(mockConfig["JwtSettings:Audience"]);

            _authController = new AuthController(_mockConfiguration.Object);
        }

        [Test]
        public void Token_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var model = new LoginModel { Username = "wronguser", Password = "wrongpassword" };

            // Act
            var result = _authController.Token(model) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(401);
            result.Value.Should().Be("Invalid credentials");
        }

        [Test]
        public void Token_ValidCredentials_ReturnsJwtToken()
        {
            // Arrange
            var model = new LoginModel { Username = "testuser", Password = "password123" };

            // Act
            var result = _authController.Token(model) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Extract token
            var jsonString = JsonSerializer.Serialize(result.Value);
            var token = JsonSerializer.Deserialize<TokenResponse>(jsonString)?.Token;
          
            token.Should().NotBeNullOrEmpty();

            // Validate the JWT
            ValidateJwtToken(token);
        }

        private void ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("SuperSecretKey12345678901234567890");

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "TestIssuer",
                ValidAudience = "TestAudience",
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero // Prevent token being accepted after it expires
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            validatedToken.Should().BeAssignableTo<JwtSecurityToken>();
            var jwtToken = (JwtSecurityToken)validatedToken;

            jwtToken.Issuer.Should().Be("TestIssuer");
            jwtToken.Audiences.Should().Contain("TestAudience");

            var nameClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            nameClaim.Should().Be("testuser");

            jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        }
    }
}
