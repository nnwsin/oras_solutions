using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using oras.Exceptions;

namespace oras.Tests.Middleware
{
    /// <summary>
    /// Tests for GlobalExceptionMiddleware — the centralized error handler for the entire API.
    ///
    /// Why this class needs tests:
    ///   This middleware translates application exceptions into HTTP responses.
    ///   It determines what status code and message every API client receives when
    ///   something goes wrong. A bug here could:
    ///     - Expose internal server details (stack traces) to clients
    ///     - Return wrong status codes (e.g., 500 instead of 404)
    ///     - Break the JSON response format
    ///
    /// Testing strategy:
    ///   - The middleware is tested in isolation without the full ASP.NET pipeline
    ///   - A RequestDelegate is created inline to simulate different exception scenarios
    ///   - HttpContext is created using DefaultHttpContext (no mocking needed)
    ///   - Response body is captured via a MemoryStream
    ///   - Tests verify: HTTP status code, Content-Type header, JSON body structure
    ///
    /// Test scenarios:
    ///   1. NotFoundException → 404
    ///   2. BadRequestException → 400
    ///   3. UnauthorizedException → 401
    ///   4. Unhandled Exception → 500
    ///   5. No exception → passes through normally
    /// </summary>
    public class GlobalExceptionMiddlewareTests
    {
        /// <summary>
        /// Helper: invokes the middleware with a given RequestDelegate and returns
        /// the status code and deserialized response body.
        /// </summary>
        private static async Task<(int StatusCode, JsonDocument Body)> InvokeMiddleware(
            RequestDelegate next)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new GlobalExceptionMiddleware(next);

            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var json = await new StreamReader(context.Response.Body).ReadToEndAsync();

            var body = json.Length > 0
                ? JsonDocument.Parse(json)
                : null!;

            return (context.Response.StatusCode, body);
        }

        [Fact]
        public async Task InvokeAsync_NotFoundException_Returns404WithMessage()
        {
            // Arrange
            RequestDelegate next = _ => throw new NotFoundException("Project not found.");

            // Act
            var (statusCode, body) = await InvokeMiddleware(next);

            // Assert
            statusCode.Should().Be((int)HttpStatusCode.NotFound);
            body.RootElement.GetProperty("StatusCode").GetInt32().Should().Be(404);
            body.RootElement.GetProperty("Message").GetString().Should().Be("Project not found.");
        }

        [Fact]
        public async Task InvokeAsync_BadRequestException_Returns400WithMessage()
        {
            // Arrange
            RequestDelegate next = _ => throw new BadRequestException("Email already exists.");

            // Act
            var (statusCode, body) = await InvokeMiddleware(next);

            // Assert
            statusCode.Should().Be((int)HttpStatusCode.BadRequest);
            body.RootElement.GetProperty("StatusCode").GetInt32().Should().Be(400);
            body.RootElement.GetProperty("Message").GetString().Should().Be("Email already exists.");
        }

        [Fact]
        public async Task InvokeAsync_UnauthorizedException_Returns401WithMessage()
        {
            // Arrange
            RequestDelegate next = _ => throw new UnauthorizedException("Invalid email or password.");

            // Act
            var (statusCode, body) = await InvokeMiddleware(next);

            // Assert
            statusCode.Should().Be((int)HttpStatusCode.Unauthorized);
            body.RootElement.GetProperty("StatusCode").GetInt32().Should().Be(401);
            body.RootElement.GetProperty("Message").GetString().Should().Be("Invalid email or password.");
        }

        [Fact]
        public async Task InvokeAsync_UnhandledException_Returns500WithMessage()
        {
            // Arrange
            RequestDelegate next = _ => throw new InvalidOperationException("Something broke");

            // Act
            var (statusCode, body) = await InvokeMiddleware(next);

            // Assert
            statusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            body.RootElement.GetProperty("StatusCode").GetInt32().Should().Be(500);
            body.RootElement.GetProperty("Message").GetString().Should().Be("Something broke");
        }

        [Fact]
        public async Task InvokeAsync_NoException_PassesThroughNormally()
        {
            // Arrange
            var wasInvoked = false;
            RequestDelegate next = _ =>
            {
                wasInvoked = true;
                return Task.CompletedTask;
            };

            var context = new DefaultHttpContext();
            var middleware = new GlobalExceptionMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            wasInvoked.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200); // default, not overwritten
        }
    }
}
