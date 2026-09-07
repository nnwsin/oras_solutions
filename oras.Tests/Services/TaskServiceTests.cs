using FluentAssertions;
using Moq;
using oras.DTOs.Tasks;
using oras.Enums;
using oras.Exceptions;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Services;

namespace oras.Tests.Services
{
    /// <summary>
    /// Tests for TaskService — the service responsible for task CRUD with cross-entity validation.
    ///
    /// Why this class needs tests:
    ///   TaskService validates that both a Project and a User (assignee) exist before
    ///   creating a task. This is a critical referential integrity check at the application
    ///   level. It also supports filtered queries (by projectId, status, assigneeId)
    ///   and soft deletes.
    ///
    /// Testing strategy:
    ///   - ITaskRepository, IProjectRepository, IUserRepository are all mocked
    ///   - Tests verify: correct DTO mapping, entity existence guards, soft delete behavior
    ///   - Filters are tested by verifying parameters are passed through correctly
    ///
    /// Test scenarios:
    ///   1.  GetAllTasksAsync — returns mapped DTOs
    ///   2.  GetTaskByIdAsync — existing task returns mapped DTO
    ///   3.  GetTaskByIdAsync — non-existent task throws NotFoundException
    ///   4.  CreateTaskAsync — valid request creates task with correct mapping
    ///   5.  CreateTaskAsync — non-existent project throws NotFoundException
    ///   6.  CreateTaskAsync — non-existent assignee throws NotFoundException
    ///   7.  UpdateTaskAsync — valid request updates all mutable fields
    ///   8.  UpdateTaskAsync — non-existent task throws NotFoundException
    ///   9.  DeleteTaskAsync — existing task sets IsDeleted to true
    ///   10. DeleteTaskAsync — non-existent task throws NotFoundException
    /// </summary>
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _taskService = new TaskService(
                _taskRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _userRepositoryMock.Object
            );
        }

        // =========================================================
        // GetAllTasksAsync
        // =========================================================

        [Fact]
        public async Task GetAllTasksAsync_ReturnsMappedDtos()
        {
            // Arrange
            var dueDate = DateTime.UtcNow.AddDays(7);

            var tasks = new List<AssignedTask>
            {
                new AssignedTask
                {
                    TaskId = 1,
                    Title = "Build API",
                    Description = "REST endpoints",
                    Status = AssignedTaskStatus.InProgress,
                    DueDate = dueDate,
                    Priority = "High",
                    ProjectId = 10,
                    AssigneeId = 20
                }
            };

            _taskRepositoryMock
                .Setup(x => x.GetFilteredTasksAsync(10, AssignedTaskStatus.InProgress, null))
                .ReturnsAsync(tasks);

            // Act
            var result = await _taskService.GetAllTasksAsync(10, AssignedTaskStatus.InProgress, null);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new TaskResponseDto
                {
                    TaskId = 1,
                    Title = "Build API",
                    Description = "REST endpoints",
                    Status = AssignedTaskStatus.InProgress,
                    DueDate = dueDate,
                    Priority = "High",
                    ProjectId = 10,
                    AssigneeId = 20
                });
        }

        // =========================================================
        // GetTaskByIdAsync
        // =========================================================

        [Fact]
        public async Task GetTaskByIdAsync_WhenTaskExists_ReturnsMappedDto()
        {
            // Arrange
            var dueDate = DateTime.UtcNow.AddDays(5);

            var task = new AssignedTask
            {
                TaskId = 1,
                Title = "Fix bug",
                Description = "Login page crash",
                Status = AssignedTaskStatus.Pending,
                DueDate = dueDate,
                Priority = "Medium",
                ProjectId = 5,
                AssigneeId = 3
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(task);

            // Act
            var result = await _taskService.GetTaskByIdAsync(1);

            // Assert
            result.Should().BeEquivalentTo(new TaskResponseDto
            {
                TaskId = 1,
                Title = "Fix bug",
                Description = "Login page crash",
                Status = AssignedTaskStatus.Pending,
                DueDate = dueDate,
                Priority = "Medium",
                ProjectId = 5,
                AssigneeId = 3
            });
        }

        [Fact]
        public async Task GetTaskByIdAsync_WhenTaskDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((AssignedTask?)null);

            // Act
            var act = () => _taskService.GetTaskByIdAsync(999);

            // Assert
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.Message.Should().Be("Task not found.");
        }

        // =========================================================
        // CreateTaskAsync
        // =========================================================

        [Fact]
        public async Task CreateTaskAsync_ValidRequest_CreatesTaskWithCorrectMapping()
        {
            // Arrange
            var dueDate = DateTime.UtcNow.AddDays(14);

            var dto = new CreateTaskDto
            {
                Title = "New Feature",
                Description = "Implement dashboard",
                Status = AssignedTaskStatus.Pending,
                DueDate = dueDate,
                Priority = "High",
                ProjectId = 1,
                AssigneeId = 2
            };

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Project { ProjectId = 1 });

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(new User { UserId = 2 });

            _taskRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<AssignedTask>()))
                .Returns(Task.CompletedTask);

            _taskRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.CreateTaskAsync(dto);

            // Assert
            result.Title.Should().Be("New Feature");
            result.Description.Should().Be("Implement dashboard");
            result.Status.Should().Be(AssignedTaskStatus.Pending);
            result.DueDate.Should().Be(dueDate);
            result.Priority.Should().Be("High");
            result.ProjectId.Should().Be(1);
            result.AssigneeId.Should().Be(2);
        }

        /// <summary>
        /// Verifies that creating a task for a non-existent project is rejected.
        /// This prevents orphaned tasks from being created.
        /// The project check happens BEFORE the user check — if both are missing,
        /// the error should be about the project.
        /// </summary>
        [Fact]
        public async Task CreateTaskAsync_ProjectDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new CreateTaskDto
            {
                Title = "Task",
                ProjectId = 999,
                AssigneeId = 1,
                Status = AssignedTaskStatus.Pending,
                Priority = "Low",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Project?)null);

            // Act
            var act = () => _taskService.CreateTaskAsync(dto);

            // Assert
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.Message.Should().Be("Project not found.");

            // User should not be checked if project doesn't exist
            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AssignedTask>()), Times.Never);
        }

        /// <summary>
        /// Verifies that creating a task for a non-existent assignee is rejected.
        /// This test also implicitly verifies the check ordering: project is validated
        /// first, then the user.
        /// </summary>
        [Fact]
        public async Task CreateTaskAsync_AssigneeDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new CreateTaskDto
            {
                Title = "Task",
                ProjectId = 1,
                AssigneeId = 999,
                Status = AssignedTaskStatus.Pending,
                Priority = "Low",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Project { ProjectId = 1 });

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((User?)null);

            // Act
            var act = () => _taskService.CreateTaskAsync(dto);

            // Assert
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.Message.Should().Be("User not found.");

            _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AssignedTask>()), Times.Never);
        }

        // =========================================================
        // UpdateTaskAsync
        // =========================================================

        [Fact]
        public async Task UpdateTaskAsync_ValidRequest_UpdatesAllMutableFields()
        {
            // Arrange
            var existingTask = new AssignedTask
            {
                TaskId = 1,
                Title = "Old Title",
                Description = "Old Description",
                Status = AssignedTaskStatus.Pending,
                DueDate = DateTime.UtcNow,
                Priority = "Low",
                ProjectId = 5,
                AssigneeId = 3
            };

            var newDueDate = DateTime.UtcNow.AddDays(30);
            var dto = new UpdateTaskDto
            {
                Title = "New Title",
                Description = "New Description",
                Status = AssignedTaskStatus.Completed,
                DueDate = newDueDate,
                Priority = "High"
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingTask);

            _taskRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<AssignedTask>()))
                .Returns(Task.CompletedTask);

            _taskRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.UpdateTaskAsync(1, dto);

            // Assert — verify all mutable fields were updated
            result.Title.Should().Be("New Title");
            result.Description.Should().Be("New Description");
            result.Status.Should().Be(AssignedTaskStatus.Completed);
            result.DueDate.Should().Be(newDueDate);
            result.Priority.Should().Be("High");

            // Verify immutable fields are preserved
            result.ProjectId.Should().Be(5);
            result.AssigneeId.Should().Be(3);
        }

        [Fact]
        public async Task UpdateTaskAsync_TaskDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new UpdateTaskDto
            {
                Title = "Title",
                Status = AssignedTaskStatus.Pending,
                DueDate = DateTime.UtcNow,
                Priority = "Low"
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((AssignedTask?)null);

            // Act
            var act = () => _taskService.UpdateTaskAsync(999, dto);

            // Assert
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.Message.Should().Be("Task not found.");
        }

        // =========================================================
        // DeleteTaskAsync
        // =========================================================

        [Fact]
        public async Task DeleteTaskAsync_WhenTaskExists_SetsIsDeletedTrue()
        {
            // Arrange
            var task = new AssignedTask
            {
                TaskId = 1,
                Title = "Task to delete",
                IsDeleted = false
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(task);

            _taskRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _taskService.DeleteTaskAsync(1);

            // Assert — the critical behavior: soft delete sets the flag
            task.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteTaskAsync_WhenTaskDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((AssignedTask?)null);

            // Act
            var act = () => _taskService.DeleteTaskAsync(999);

            // Assert
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.Message.Should().Be("Task not found.");
        }
    }
}
