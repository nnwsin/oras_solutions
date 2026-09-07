using FluentAssertions;
using Moq;
using oras.DTOs.User;
using oras.Enums;
using oras.Exceptions;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Services;
using oras.Services.Interfaces;

namespace oras.Tests.Services
{
    /// <summary>
    /// Tests for UserService — the service with the richest business logic in the application.
    ///
    /// Why this class needs tests:
    ///   UserService is responsible for user CRUD operations. Unlike simpler services,
    ///   it contains two unique business rules:
    ///     1. Duplicate email prevention on create (reject if email already exists)
    ///     2. Duplicate email prevention on update (reject if email taken by a *different* user,
    ///        but allow the same user to keep their own email)
    ///   It also delegates password hashing to IPasswordService during user creation.
    ///
    /// Testing strategy:
    ///   - IUserRepository is mocked to isolate service logic from database
    ///   - IPasswordService is mocked to verify hashing delegation without BCrypt dependency
    ///   - Tests verify observable behavior: return values, exception types/messages, entity state changes
    ///
    /// Test scenarios:
    ///   1.  GetAllUsersAsync — returns mapped DTOs for all users
    ///   2.  GetAllUsersAsync — empty collection returns empty
    ///   3.  GetUserByIdAsync — existing user returns correct mapped DTO
    ///   4.  GetUserByIdAsync — non-existent user throws NotFoundException
    ///   5.  CreateUserAsync — valid request creates user with hashed password and correct mapping
    ///   6.  CreateUserAsync — duplicate email throws BadRequestException
    ///   7.  UpdateUserAsync — valid request updates name and email
    ///   8.  UpdateUserAsync — non-existent user throws NotFoundException
    ///   9.  UpdateUserAsync — email taken by another user throws BadRequestException
    ///   10. UpdateUserAsync — user keeping their own email succeeds
    ///   11. DeleteUserAsync — existing user sets IsDeleted to true
    ///   12. DeleteUserAsync — non-existent user throws NotFoundException
    /// </summary>
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();

            _userService = new UserService(
                _userRepositoryMock.Object,
                _passwordServiceMock.Object
            );
        }

        // =========================================================
        // GetAllUsersAsync
        // =========================================================

        [Fact]
        public async Task GetAllUsersAsync_WhenUsersExist_ReturnsMappedDtos()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, Name = "Alice", Email = "alice@test.com", Role = UserRole.Admin },
                new User { UserId = 2, Name = "Bob", Email = "bob@test.com", Role = UserRole.Employee }
            };

            _userRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().BeEquivalentTo(new[]
            {
                new UserResponseDto { UserId = 1, Name = "Alice", Email = "alice@test.com", Role = UserRole.Admin },
                new UserResponseDto { UserId = 2, Name = "Bob", Email = "bob@test.com", Role = UserRole.Employee }
            });
        }

        [Fact]
        public async Task GetAllUsersAsync_WhenNoUsersExist_ReturnsEmptyCollection()
        {
            // Arrange
            _userRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        // =========================================================
        // GetUserByIdAsync
        // =========================================================

        [Fact]
        public async Task GetUserByIdAsync_WhenUserExists_ReturnsMappedDto()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Name = "Alice",
                Email = "alice@test.com",
                Role = UserRole.Manager
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(1);

            // Assert
            result.Should().BeEquivalentTo(new UserResponseDto
            {
                UserId = 1,
                Name = "Alice",
                Email = "alice@test.com",
                Role = UserRole.Manager
            });
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((User?)null);

            // Act
            var act = () => _userService.GetUserByIdAsync(999);

            // Assert
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.Message.Should().Be("User not found.");
        }

        // =========================================================
        // CreateUserAsync
        // =========================================================

        /// <summary>
        /// This test verifies the happy path for user creation.
        /// It's critical because it confirms:
        ///   - The password is hashed (not stored as plaintext)
        ///   - All DTO fields are correctly mapped to the entity
        ///   - The response DTO does NOT include the password
        /// </summary>
        [Fact]
        public async Task CreateUserAsync_ValidRequest_CreatesUserWithHashedPassword()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Name = "Alice",
                Email = "alice@test.com",
                Password = "plaintext123",
                Role = UserRole.Manager
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("alice@test.com"))
                .ReturnsAsync((User?)null);

            _passwordServiceMock
                .Setup(x => x.HashPassword("plaintext123"))
                .Returns("$2a$hashed_value");

            _userRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.CreateUserAsync(dto);

            // Assert
            result.Name.Should().Be("Alice");
            result.Email.Should().Be("alice@test.com");
            result.Role.Should().Be(UserRole.Manager);

            // Verify that AddAsync received an entity with the HASHED password, not plaintext
            _userRepositoryMock.Verify(
                x => x.AddAsync(It.Is<User>(u =>
                    u.Password == "$2a$hashed_value" &&
                    u.Name == "Alice" &&
                    u.Email == "alice@test.com" &&
                    u.Role == UserRole.Manager
                )),
                Times.Once
            );
        }

        /// <summary>
        /// This is the most important guard in CreateUserAsync.
        /// If this check were removed, duplicate accounts could be created,
        /// violating the unique email constraint at the database level
        /// and causing runtime exceptions.
        /// </summary>
        [Fact]
        public async Task CreateUserAsync_DuplicateEmail_ThrowsBadRequestException()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Name = "Alice",
                Email = "existing@test.com",
                Password = "password123",
                Role = UserRole.Employee
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("existing@test.com"))
                .ReturnsAsync(new User { UserId = 99, Email = "existing@test.com" });

            // Act
            var act = () => _userService.CreateUserAsync(dto);

            // Assert
            var exception = await act.Should().ThrowAsync<BadRequestException>();
            exception.Which.Message.Should().Be("Email already exists.");

            // Verify that no user was added or saved
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        // =========================================================
        // UpdateUserAsync
        // =========================================================

        [Fact]
        public async Task UpdateUserAsync_ValidRequest_UpdatesNameAndEmail()
        {
            // Arrange
            var existingUser = new User
            {
                UserId = 1,
                Name = "Old Name",
                Email = "old@test.com",
                Role = UserRole.Admin
            };

            var dto = new UpdateUserDto
            {
                Name = "New Name",
                Email = "new@test.com"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingUser);

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("new@test.com"))
                .ReturnsAsync((User?)null);

            _userRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateUserAsync(1, dto);

            // Assert
            result.Should().BeEquivalentTo(new UserResponseDto
            {
                UserId = 1,
                Name = "New Name",
                Email = "new@test.com",
                Role = UserRole.Admin
            });
        }

        [Fact]
        public async Task UpdateUserAsync_UserDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new UpdateUserDto { Name = "Name", Email = "email@test.com" };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((User?)null);

            // Act
            var act = () => _userService.UpdateUserAsync(999, dto);

            // Assert
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.Message.Should().Be("User not found.");
        }

        /// <summary>
        /// This test protects a subtle but critical business rule:
        /// When updating a user's email, the system must reject the update
        /// if another user already has that email — but must ALLOW the update
        /// if the found user is the SAME user (they're keeping their current email).
        ///
        /// The production code checks: existingUser.UserId != id
        /// If this guard were removed, users could never keep their own email during updates.
        /// </summary>
        [Fact]
        public async Task UpdateUserAsync_EmailTakenByAnotherUser_ThrowsBadRequestException()
        {
            // Arrange
            var currentUser = new User { UserId = 1, Name = "Alice", Email = "alice@test.com" };
            var otherUser = new User { UserId = 2, Email = "taken@test.com" };

            var dto = new UpdateUserDto { Name = "Alice", Email = "taken@test.com" };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(currentUser);

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("taken@test.com"))
                .ReturnsAsync(otherUser);

            // Act
            var act = () => _userService.UpdateUserAsync(1, dto);

            // Assert
            var exception = await act.Should().ThrowAsync<BadRequestException>();
            exception.Which.Message.Should().Be("Email already exists.");

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        /// <summary>
        /// Counterpart to the above test — verifies that a user CAN keep their own email.
        /// This tests the `existingUser.UserId != id` guard in the production code.
        /// Without this test, a regression that changes the guard to a simple
        /// `existingUser != null` would go undetected.
        /// </summary>
        [Fact]
        public async Task UpdateUserAsync_UserKeepsOwnEmail_Succeeds()
        {
            // Arrange
            var currentUser = new User
            {
                UserId = 1,
                Name = "Alice",
                Email = "alice@test.com",
                Role = UserRole.Employee
            };

            var dto = new UpdateUserDto { Name = "Alice Updated", Email = "alice@test.com" };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(currentUser);

            // GetByEmailAsync returns the SAME user — this must be allowed
            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("alice@test.com"))
                .ReturnsAsync(currentUser);

            _userRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateUserAsync(1, dto);

            // Assert
            result.Name.Should().Be("Alice Updated");
            result.Email.Should().Be("alice@test.com");
        }

        // =========================================================
        // DeleteUserAsync
        // =========================================================

        [Fact]
        public async Task DeleteUserAsync_WhenUserExists_SetsIsDeletedTrue()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Name = "Alice",
                IsDeleted = false
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _userService.DeleteUserAsync(1);

            // Assert — the critical behavior: soft delete sets the flag
            user.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((User?)null);

            // Act
            var act = () => _userService.DeleteUserAsync(999);

            // Assert
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.Message.Should().Be("User not found.");
        }
    }
}
