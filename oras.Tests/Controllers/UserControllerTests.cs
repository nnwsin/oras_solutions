using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using oras.Controllers;
using oras.DTOs.User;
using oras.Enums;
using oras.Services.Interfaces;

namespace oras.Tests.Controllers
{
    /// <summary>
    /// Tests for UserController — verifies correct HTTP response types and status codes.
    ///
    /// Why this class needs tests:
    ///   While the controller is a thin delegate, it is the layer that determines
    ///   what HTTP contract clients receive. These tests verify:
    ///     - GET returns OkObjectResult (200)
    ///     - POST returns CreatedAtActionResult (201) with correct route values
    ///     - PUT returns OkObjectResult (200)
    ///     - DELETE returns NoContentResult (204)
    ///
    /// Testing strategy:
    ///   - IUserService is mocked — business logic is tested in UserServiceTests
    ///   - Tests verify HTTP response types and payload, not service interactions
    ///   - One happy-path test per action is sufficient since error handling
    ///     is done by GlobalExceptionMiddleware (already tested)
    /// </summary>
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UserController(_userServiceMock.Object);
        }

        // =========================================================
        // GET api/User
        // =========================================================

        [Fact]
        public async Task GetAllUsers_ReturnsOkWithUsers()
        {
            // Arrange
            var users = new List<UserResponseDto>
            {
                new() { UserId = 1, Name = "Alice", Email = "alice@test.com", Role = UserRole.Admin },
                new() { UserId = 2, Name = "Bob", Email = "bob@test.com", Role = UserRole.Employee }
            };

            _userServiceMock
                .Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(users);
        }

        [Fact]
        public async Task GetAllUsers_WhenEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(Enumerable.Empty<UserResponseDto>());

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            (ok.Value as IEnumerable<UserResponseDto>).Should().BeEmpty();
        }

        // =========================================================
        // GET api/User/{id}
        // =========================================================

        [Fact]
        public async Task GetUserById_ExistingUser_ReturnsOkWithUser()
        {
            // Arrange
            var user = new UserResponseDto
            {
                UserId = 1,
                Name = "Alice",
                Email = "alice@test.com",
                Role = UserRole.Manager
            };

            _userServiceMock
                .Setup(s => s.GetUserByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(1);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(user);
        }

        // =========================================================
        // POST api/User
        // =========================================================

        [Fact]
        public async Task CreateUser_ValidDto_ReturnsCreatedAtActionWithUser()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Name = "Alice",
                Email = "alice@test.com",
                Password = "password123",
                Role = UserRole.Employee
            };

            var createdUser = new UserResponseDto
            {
                UserId = 42,
                Name = "Alice",
                Email = "alice@test.com",
                Role = UserRole.Employee
            };

            _userServiceMock
                .Setup(s => s.CreateUserAsync(dto))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(dto);

            // Assert
            var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.StatusCode.Should().Be(201);
            created.ActionName.Should().Be(nameof(UserController.GetUserById));
            created.RouteValues!["id"].Should().Be(42);
            created.Value.Should().BeEquivalentTo(createdUser);
        }

        // =========================================================
        // PUT api/User/{id}
        // =========================================================

        [Fact]
        public async Task UpdateUser_ValidDto_ReturnsOkWithUpdatedUser()
        {
            // Arrange
            var dto = new UpdateUserDto { Name = "Updated", Email = "updated@test.com" };

            var updatedUser = new UserResponseDto
            {
                UserId = 1,
                Name = "Updated",
                Email = "updated@test.com",
                Role = UserRole.Admin
            };

            _userServiceMock
                .Setup(s => s.UpdateUserAsync(1, dto))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser(1, dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(updatedUser);
        }

        // =========================================================
        // DELETE api/User/{id}
        // =========================================================

        [Fact]
        public async Task DeleteUser_ExistingId_ReturnsNoContent()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.DeleteUserAsync(1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteUser(1);

            // Assert
            result.Should().BeOfType<NoContentResult>()
                .Which.StatusCode.Should().Be(204);
        }
    }
}
