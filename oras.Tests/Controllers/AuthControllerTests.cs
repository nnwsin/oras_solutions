using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using oras.Auth.Controllers;
using oras.Auth.Dto;
using oras.Auth.Interfaces;

namespace oras.Tests.Controllers
{
    /// <summary>
    /// Tests for AuthController — verifies correct HTTP response types for auth endpoints.
    ///
    /// Why this class needs tests:
    ///   AuthController has a slightly different response pattern than the CRUD controllers:
    ///     - Register returns Ok with a success message (not CreatedAtAction)
    ///     - Login returns Ok with the AuthResponseDto (token + expiration)
    ///   These are the only public (AllowAnonymous) endpoints in the API.
    ///
    /// Testing strategy:
    ///   - IAuthService is mocked — auth business logic is tested in AuthServiceTests
    ///   - Tests verify HTTP response types and payload structure
    ///   - Error scenarios (duplicate email, wrong password) are handled by
    ///     GlobalExceptionMiddleware and tested there
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        // =========================================================
        // POST api/Auth/Register
        // =========================================================

        [Fact]
        public async Task Register_ValidDto_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Name = "New User",
                Email = "newuser@test.com",
                Password = "SecurePass123"
            };

            _authServiceMock
                .Setup(s => s.RegisterAsync(dto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);

            // Verify the response contains the expected message
            var responseJson = System.Text.Json.JsonSerializer.Serialize(ok.Value);
            responseJson.Should().Contain("User registered successfully.");
        }

        // =========================================================
        // POST api/Auth/Login
        // =========================================================

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithTokenResponse()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "alice@test.com",
                Password = "correct_password"
            };

            var expiration = DateTime.UtcNow.AddMinutes(60);

            var authResponse = new AuthResponseDto
            {
                Token = "jwt.token.value",
                Expiration = expiration
            };

            _authServiceMock
                .Setup(s => s.LoginAsync(dto))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(authResponse);
        }
    }
}
