using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Mvc;


namespace GLTest.Tests.Middleware
{
    [TestFixture]
    public class GlobalExceptionMiddlewareTests
    {
        private Mock<ILogger<GlobalExceptionMiddleware>> _mockLogger;
        private RequestDelegate _nextDelegate;
        private GlobalExceptionMiddleware _middleware;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<GlobalExceptionMiddleware>>();
            _nextDelegate = Mock.Of<RequestDelegate>(); // Mock the next delegate
            _middleware = new GlobalExceptionMiddleware(_nextDelegate, _mockLogger.Object);
        }

        private async Task<HttpResponse> InvokeMiddlewareWithException(Exception exception)
        {
            var context = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            var middleware = new GlobalExceptionMiddleware(
                async ctx => throw exception,
                _mockLogger.Object
            );

            await middleware.Invoke(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            return context.Response;
        }
             

        [Test]
        public async Task Invoke_ThrowsUnhandledException_ReturnsServerError()
        {
            // Arrange
            var unexpectedException = new Exception("Unexpected error");

            // Act
            var response = await InvokeMiddlewareWithException(unexpectedException);

            // Assert
            response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
            var jsonResponse = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            jsonResponse.Should().NotBeNull();
            jsonResponse!.Title.Should().Be("Internal Server Error");
            jsonResponse!.Detail.Should().Be("An unexpected error occurred.");
        }
    }



  

}
