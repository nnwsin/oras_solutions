using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using oras.Controllers;
using oras.DTOs.Tasks;
using oras.Enums;
using oras.Services.Interfaces;

namespace oras.Tests.Controllers
{
    public class TaskControllerTests
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly TaskController _taskController;

        public TaskControllerTests()
        {
            _taskServiceMock = new Mock<ITaskService>();

            _taskController = new TaskController(
                _taskServiceMock.Object
            );
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET api/Task  (GetAllTasks)
        // ──────────────────────────

        [Fact]
        public async Task GetAllTasks_NoFilters_ReturnsOkWithAllTasks()
        {
            // Arrange
            var tasks = new List<TaskResponseDto>
            {
                new() { TaskId = 1, Title = "Task A", Status = AssignedTaskStatus.Pending,   Priority = "High",   ProjectId = 1, AssigneeId = 1, DueDate = DateTime.UtcNow.AddDays(3) },
                new() { TaskId = 2, Title = "Task B", Status = AssignedTaskStatus.InProgress, Priority = "Medium", ProjectId = 2, AssigneeId = 2, DueDate = DateTime.UtcNow.AddDays(5) }
            };

            _taskServiceMock
                .Setup(s => s.GetAllTasksAsync(null, null, null))
                .ReturnsAsync(tasks);

            // Act
            var result = await _taskController.GetAllTasks(null, null, null);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(tasks);
        }



        [Fact]
        public async Task GetAllTasks_FilteredByProjectId_ReturnsOkWithMatchingTasks()
        {
            // Arrange
            var tasks = new List<TaskResponseDto>
            {
                new() { TaskId = 1, Title = "Project Task", Status = AssignedTaskStatus.Pending, Priority = "High", ProjectId = 10, AssigneeId = 1, DueDate = DateTime.UtcNow.AddDays(2) }
            };

            _taskServiceMock
                .Setup(s => s.GetAllTasksAsync(10, null, null))
                .ReturnsAsync(tasks);

            // Act
            var result = await _taskController.GetAllTasks(projectId: 10, status: null, assigneeId: null);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(tasks);
        }

        [Fact]
        public async Task GetAllTasks_FilteredByStatus_ReturnsOkWithMatchingTasks()
        {
            // Arrange
            var tasks = new List<TaskResponseDto>
            {
                new() { TaskId = 3, Title = "Completed Task", Status = AssignedTaskStatus.Completed, Priority = "Low", ProjectId = 1, AssigneeId = 5, DueDate = DateTime.UtcNow.AddDays(-1) }
            };

            _taskServiceMock
                .Setup(s => s.GetAllTasksAsync(null, AssignedTaskStatus.Completed, null))
                .ReturnsAsync(tasks);

            // Act
            var result = await _taskController.GetAllTasks(projectId: null, status: AssignedTaskStatus.Completed, assigneeId: null);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(tasks);
        }

        [Fact]
        public async Task GetAllTasks_FilteredByAssigneeId_ReturnsOkWithMatchingTasks()
        {
            // Arrange
            var tasks = new List<TaskResponseDto>
            {
                new() { TaskId = 4, Title = "Assignee Task", Status = AssignedTaskStatus.InProgress, Priority = "Medium", ProjectId = 2, AssigneeId = 7, DueDate = DateTime.UtcNow.AddDays(4) }
            };

            _taskServiceMock
                .Setup(s => s.GetAllTasksAsync(null, null, 7))
                .ReturnsAsync(tasks);

            // Act
            var result = await _taskController.GetAllTasks(projectId: null, status: null, assigneeId: 7);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(tasks);
        }

        [Fact]
        public async Task GetAllTasks_FilteredByProjectIdAndStatus_ReturnsOkWithMatchingTasks()
        {
            // Arrange
            var tasks = new List<TaskResponseDto>
            {
                new() { TaskId = 5, Title = "Multi-Filter Task", Status = AssignedTaskStatus.Cancelled, Priority = "High", ProjectId = 3, AssigneeId = 2, DueDate = DateTime.UtcNow.AddDays(10) }
            };

            _taskServiceMock
                .Setup(s => s.GetAllTasksAsync(3, AssignedTaskStatus.Cancelled, null))
                .ReturnsAsync(tasks);

            // Act
            var result = await _taskController.GetAllTasks(projectId: 3, status: AssignedTaskStatus.Cancelled, assigneeId: null);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(tasks);
        }

        [Fact]
        public async Task GetAllTasks_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            _taskServiceMock
                .Setup(s => s.GetAllTasksAsync(null, null, null))
                .ReturnsAsync(Enumerable.Empty<TaskResponseDto>());

            // Act
            var result = await _taskController.GetAllTasks(null, null, null);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            (ok.Value as IEnumerable<TaskResponseDto>).Should().BeEmpty();
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET api/Task/{id}  (GetTaskById)
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetTaskById_ExistingId_ReturnsOkWithTask()
        {
            // Arrange
            var task = new TaskResponseDto
            {
                TaskId    = 1,
                Title     = "Fix login bug",
                Description = "Users can't login via SSO",
                Status    = AssignedTaskStatus.InProgress,
                DueDate   = DateTime.UtcNow.AddDays(7),
                Priority  = "High",
                ProjectId = 1,
                AssigneeId = 2
            };

            _taskServiceMock
                .Setup(s => s.GetTaskByIdAsync(1))
                .ReturnsAsync(task);

            // Act
            var result = await _taskController.GetTaskById(1);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(task);
        }

        [Fact]
        public async Task GetTaskById_ServiceIsCalled_WithCorrectId()
        {
            // Arrange
            var task = new TaskResponseDto { TaskId = 42, Title = "Sample", Status = AssignedTaskStatus.Pending, Priority = "Low", ProjectId = 1, AssigneeId = 1, DueDate = DateTime.UtcNow };

            _taskServiceMock
                .Setup(s => s.GetTaskByIdAsync(42))
                .ReturnsAsync(task);

            // Act
            await _taskController.GetTaskById(42);

            // Assert
            _taskServiceMock.Verify(s => s.GetTaskByIdAsync(42), Times.Once);
        }

        // ──────────────────────────────────────────────────────────────────────
        // POST api/Task  (CreateTask)
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateTask_ValidDto_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var dto = new CreateTaskDto
            {
                Title      = "Implement auth module",
                Description = "JWT-based authentication",
                Status     = AssignedTaskStatus.Pending,
                DueDate    = DateTime.UtcNow.AddDays(14),
                ProjectId  = 1,
                AssigneeId = 3,
                Priority   = "High"
            };

            var createdTask = new TaskResponseDto
            {
                TaskId     = 99,
                Title      = dto.Title,
                Description = dto.Description,
                Status     = dto.Status,
                DueDate    = dto.DueDate,
                Priority   = dto.Priority,
                ProjectId  = dto.ProjectId,
                AssigneeId = dto.AssigneeId
            };

            _taskServiceMock
                .Setup(s => s.CreateTaskAsync(dto))
                .ReturnsAsync(createdTask);

            // Act
            var result = await _taskController.CreateTask(dto);

            // Assert
            var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.StatusCode.Should().Be(201);
            created.ActionName.Should().Be(nameof(TaskController.GetTaskById));
            created.RouteValues!["id"].Should().Be(createdTask.TaskId);
            created.Value.Should().BeEquivalentTo(createdTask);
        }

        [Fact]
        public async Task CreateTask_ServiceIsCalled_WithCorrectDto()
        {
            // Arrange
            var dto = new CreateTaskDto
            {
                Title      = "Test Task",
                Status     = AssignedTaskStatus.Pending,
                DueDate    = DateTime.UtcNow.AddDays(5),
                ProjectId  = 2,
                AssigneeId = 4,
                Priority   = "Medium"
            };

            var createdTask = new TaskResponseDto { TaskId = 10, Title = dto.Title, Status = dto.Status, DueDate = dto.DueDate, Priority = dto.Priority, ProjectId = dto.ProjectId, AssigneeId = dto.AssigneeId };

            _taskServiceMock
                .Setup(s => s.CreateTaskAsync(dto))
                .ReturnsAsync(createdTask);

            // Act
            await _taskController.CreateTask(dto);

            // Assert
            _taskServiceMock.Verify(s => s.CreateTaskAsync(dto), Times.Once);
        }

        [Fact]
        public async Task CreateTask_TaskIdPresentInRouteValues()
        {
            // Arrange
            var dto = new CreateTaskDto
            {
                Title      = "Route Check Task",
                Status     = AssignedTaskStatus.InProgress,
                DueDate    = DateTime.UtcNow.AddDays(3),
                ProjectId  = 1,
                AssigneeId = 1,
                Priority   = "Low"
            };

            var createdTask = new TaskResponseDto { TaskId = 55, Title = dto.Title, Status = dto.Status, DueDate = dto.DueDate, Priority = dto.Priority, ProjectId = dto.ProjectId, AssigneeId = dto.AssigneeId };

            _taskServiceMock
                .Setup(s => s.CreateTaskAsync(dto))
                .ReturnsAsync(createdTask);

            // Act
            var result = await _taskController.CreateTask(dto);

            // Assert
            var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.RouteValues.Should().ContainKey("id");
            created.RouteValues!["id"].Should().Be(55);
        }

        // ──────────────────────────────────────────────────────────────────────
        // PUT api/Task/{id}  (UpdateTask)
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateTask_ValidIdAndDto_ReturnsOkWithUpdatedTask()
        {
            // Arrange
            var dto = new UpdateTaskDto
            {
                Title      = "Updated Title",
                Description = "Updated description",
                Status     = AssignedTaskStatus.Completed,
                DueDate    = DateTime.UtcNow.AddDays(1),
                Priority   = "Low"
            };

            var updatedTask = new TaskResponseDto
            {
                TaskId      = 1,
                Title       = dto.Title,
                Description = dto.Description,
                Status      = dto.Status,
                DueDate     = dto.DueDate,
                Priority    = dto.Priority,
                ProjectId   = 1,
                AssigneeId  = 2
            };

            _taskServiceMock
                .Setup(s => s.UpdateTaskAsync(1, dto))
                .ReturnsAsync(updatedTask);

            // Act
            var result = await _taskController.UpdateTask(1, dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(updatedTask);
        }

        [Fact]
        public async Task UpdateTask_ServiceIsCalled_WithCorrectIdAndDto()
        {
            // Arrange
            var dto = new UpdateTaskDto
            {
                Title    = "Check Service Call",
                Status   = AssignedTaskStatus.InProgress,
                DueDate  = DateTime.UtcNow.AddDays(2),
                Priority = "High"
            };

            var updatedTask = new TaskResponseDto { TaskId = 7, Title = dto.Title, Status = dto.Status, DueDate = dto.DueDate, Priority = dto.Priority, ProjectId = 1, AssigneeId = 1 };

            _taskServiceMock
                .Setup(s => s.UpdateTaskAsync(7, dto))
                .ReturnsAsync(updatedTask);

            // Act
            await _taskController.UpdateTask(7, dto);

            // Assert
            _taskServiceMock.Verify(s => s.UpdateTaskAsync(7, dto), Times.Once);
        }

        [Fact]
        public async Task UpdateTask_StatusChangedToCompleted_ReturnsOkWithCompletedStatus()
        {
            // Arrange
            var dto = new UpdateTaskDto
            {
                Title    = "Done Task",
                Status   = AssignedTaskStatus.Completed,
                DueDate  = DateTime.UtcNow,
                Priority = "Medium"
            };

            var updatedTask = new TaskResponseDto { TaskId = 3, Title = dto.Title, Status = AssignedTaskStatus.Completed, DueDate = dto.DueDate, Priority = dto.Priority, ProjectId = 1, AssigneeId = 1 };

            _taskServiceMock
                .Setup(s => s.UpdateTaskAsync(3, dto))
                .ReturnsAsync(updatedTask);

            // Act
            var result = await _taskController.UpdateTask(3, dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var value = ok.Value.Should().BeOfType<TaskResponseDto>().Subject;
            value.Status.Should().Be(AssignedTaskStatus.Completed);
        }

        // ──────────────────────────────────────────────────────────────────────
        // DELETE api/Task/{id}  (DeleteTask)
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteTask_ExistingId_ReturnsNoContent()
        {
            // Arrange
            _taskServiceMock
                .Setup(s => s.DeleteTaskAsync(1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskController.DeleteTask(1);

            // Assert
            result.Should().BeOfType<NoContentResult>()
                  .Which.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteTask_ServiceIsCalled_WithCorrectId()
        {
            // Arrange
            _taskServiceMock
                .Setup(s => s.DeleteTaskAsync(5))
                .Returns(Task.CompletedTask);

            // Act
            await _taskController.DeleteTask(5);

            // Assert
            _taskServiceMock.Verify(s => s.DeleteTaskAsync(5), Times.Once);
        }

        [Fact]
        public async Task DeleteTask_IsCalledExactlyOnce_NotMultipleTimes()
        {
            // Arrange
            _taskServiceMock
                .Setup(s => s.DeleteTaskAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await _taskController.DeleteTask(3);

            // Assert
            _taskServiceMock.Verify(s => s.DeleteTaskAsync(It.IsAny<int>()), Times.Once);
        }
    }
}
