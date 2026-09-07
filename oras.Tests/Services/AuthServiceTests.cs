using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using oras.Auth.Dto;
using oras.Auth.Interfaces;
using oras.Auth.Services;
using oras.Enums;
using oras.Exceptions;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Services.Interfaces;

namespace oras.Tests.Services
{
    /// <summary>
    /// Tests for AuthService — the most security-critical service in the application.
    ///
    /// Why this class needs tests:
    ///   AuthService handles user registration and login. A bug here could allow:
    ///     - Duplicate account creation (bypassing email uniqueness)
    ///     - Plaintext password storage (forgetting to hash)
    ///     - Authentication with wrong credentials
    ///     - Wrong role assignment during registration
    ///   These are high-impact bugs that automated tests must catch.
    ///
    /// Testing strategy:
    ///   - IUserRepository is mocked (no database)
    ///   - IPasswordService is mocked (no BCrypt dependency; verify hashing is called)
    ///   - IJwtService is mocked (verify token generation is called with the right user)
    ///   - IConfiguration is built in-memory to supply JWT settings
    ///   - Tests verify observable behavior: exceptions, return values, entity state
    ///
    /// Test scenarios:
    ///   1. RegisterAsync — valid registration hashes password and assigns Employee role
    ///   2. RegisterAsync — duplicate email throws BadRequestException
    ///   3. LoginAsync — valid credentials returns token with expiration
    ///   4. LoginAsync — non-existent email throws UnauthorizedException
    ///   5. LoginAsync — wrong password throws UnauthorizedException
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _jwtServiceMock = new Mock<IJwtService>();

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:ExpiryInMinutes"] = "60"
                })
                .Build();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _passwordServiceMock.Object,
                _jwtServiceMock.Object,
                _configuration
            );
        }

        // =========================================================
        // RegisterAsync
        // =========================================================

        /// <summary>
        /// Verifies the happy path of registration. Three critical behaviors:
        ///   1. Password is hashed before storage
        ///   2. Role is hardcoded to Employee (not taken from input)
        ///   3. User is persisted via AddAsync + SaveChangesAsync
        ///
        /// The role hardcoding is a deliberate security decision — users cannot
        /// self-register as Admin or Manager.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_ValidRequest_CreatesUserWithHashedPasswordAndEmployeeRole()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Name = "New User",
                Email = "newuser@test.com",
                Password = "SecurePass123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("newuser@test.com"))
                .ReturnsAsync((User?)null);

            _passwordServiceMock
                .Setup(x => x.HashPassword("SecurePass123"))
                .Returns("$2a$hashed");

            _userRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _authService.RegisterAsync(dto);

            // Assert — verify the entity passed to AddAsync has correct values
            _userRepositoryMock.Verify(
                x => x.AddAsync(It.Is<User>(u =>
                    u.Name == "New User" &&
                    u.Email == "newuser@test.com" &&
                    u.Password == "$2a$hashed" &&
                    u.Role == UserRole.Employee
                )),
                Times.Once
            );

            _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsBadRequestException()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Name = "User",
                Email = "existing@test.com",
                Password = "password"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("existing@test.com"))
                .ReturnsAsync(new User { UserId = 1, Email = "existing@test.com" });

            // Act
            var act = () => _authService.RegisterAsync(dto);

            // Assert
            var exception = await act.Should().ThrowAsync<BadRequestException>();
            exception.Which.Message.Should().Be("Email already exists.");

            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        // =========================================================
        // LoginAsync
        // =========================================================

        /// <summary>
        /// Verifies the complete login flow:
        ///   1. User is looked up by email
        ///   2. Password is verified against the stored hash
        ///   3. JWT token is generated for the found user
        ///   4. Response contains token and an expiration time based on configuration
        /// </summary>
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsTokenWithExpiration()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Name = "Alice",
                Email = "alice@test.com",
                Password = "$2a$stored_hash",
                Role = UserRole.Manager
            };

            var dto = new LoginDto
            {
                Email = "alice@test.com",
                Password = "correct_password"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("alice@test.com"))
                .ReturnsAsync(user);

            _passwordServiceMock
                .Setup(x => x.VerifyPassword("correct_password", "$2a$stored_hash"))
                .Returns(true);

            _jwtServiceMock
                .Setup(x => x.GenerateToken(user))
                .Returns("jwt.token.here");

            var beforeAct = DateTime.UtcNow;

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            result.Token.Should().Be("jwt.token.here");

            // Expiration should be approximately 60 minutes from now (as configured)
            result.Expiration.Should().BeAfter(beforeAct.AddMinutes(59));
            result.Expiration.Should().BeBefore(DateTime.UtcNow.AddMinutes(61));
        }

        [Fact]
        public async Task LoginAsync_NonExistentEmail_ThrowsUnauthorizedException()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "nobody@test.com",
                Password = "password"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("nobody@test.com"))
                .ReturnsAsync((User?)null);

            // Act
            var act = () => _authService.LoginAsync(dto);

            // Assert
            var exception = await act.Should().ThrowAsync<UnauthorizedException>();
            exception.Which.Message.Should().Be("Invalid email or password.");

            // Password should never be checked if user doesn't exist
            _passwordServiceMock.Verify(
                x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        /// <summary>
        /// Critical security test: verifies that a wrong password is rejected
        /// even when the email is correct. The error message must be the same
        /// as for non-existent email to prevent email enumeration attacks.
        /// </summary>
        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Email = "alice@test.com",
                Password = "$2a$stored_hash"
            };

            var dto = new LoginDto
            {
                Email = "alice@test.com",
                Password = "wrong_password"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("alice@test.com"))
                .ReturnsAsync(user);

            _passwordServiceMock
                .Setup(x => x.VerifyPassword("wrong_password", "$2a$stored_hash"))
                .Returns(false);

            // Act
            var act = () => _authService.LoginAsync(dto);

            // Assert
            var exception = await act.Should().ThrowAsync<UnauthorizedException>();
            exception.Which.Message.Should().Be("Invalid email or password.");

            // Token should never be generated for failed auth
            _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        }
    }
}
