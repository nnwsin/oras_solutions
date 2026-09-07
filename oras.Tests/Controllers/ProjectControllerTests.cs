using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using oras.Controllers;
using oras.DTOs.Project;
using oras.Services.Interfaces;

namespace oras.Tests.Controllers
{
    /// <summary>
    /// Tests for ProjectController — verifies correct HTTP response types and status codes.
    ///
    /// Why this class needs tests:
    ///   The controller determines the HTTP contract clients rely on.
    ///   These tests verify:
    ///     - GET returns 200 with the project data
    ///     - POST returns 201 with CreatedAtAction pointing to GetProjectById
    ///     - PUT returns 200 with the updated project
    ///     - DELETE returns 204 (No Content)
    ///
    /// Testing strategy:
    ///   - IProjectService is mocked — business logic is tested in ProjectServiceTests
    ///   - One focused test per action verifying the HTTP response type and payload
    /// </summary>
    public class ProjectControllerTests
    {
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly ProjectController _controller;

        public ProjectControllerTests()
        {
            _projectServiceMock = new Mock<IProjectService>();
            _controller = new ProjectController(_projectServiceMock.Object);
        }

        // =========================================================
        // GET api/Project
        // =========================================================

        [Fact]
        public async Task GetAllProjects_ReturnsOkWithProjects()
        {
            // Arrange
            var projects = new List<ProjectResponseDto>
            {
                new() { ProjectId = 1, ProjectName = "Alpha", OwnerId = 10 },
                new() { ProjectId = 2, ProjectName = "Beta", OwnerId = 20 }
            };

            _projectServiceMock
                .Setup(s => s.GetAllProjectsAsync())
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.GetAllProjects();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(projects);
        }

        [Fact]
        public async Task GetAllProjects_WhenEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            _projectServiceMock
                .Setup(s => s.GetAllProjectsAsync())
                .ReturnsAsync(Enumerable.Empty<ProjectResponseDto>());

            // Act
            var result = await _controller.GetAllProjects();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            (ok.Value as IEnumerable<ProjectResponseDto>).Should().BeEmpty();
        }

        // =========================================================
        // GET api/Project/{id}
        // =========================================================

        [Fact]
        public async Task GetProjectById_ExistingProject_ReturnsOkWithProject()
        {
            // Arrange
            var project = new ProjectResponseDto
            {
                ProjectId = 1,
                ProjectName = "Alpha",
                OwnerId = 10
            };

            _projectServiceMock
                .Setup(s => s.GetProjectByIdAsync(1))
                .ReturnsAsync(project);

            // Act
            var result = await _controller.GetProjectById(1);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(project);
        }

        // =========================================================
        // POST api/Project
        // =========================================================

        [Fact]
        public async Task CreateProject_ValidDto_ReturnsCreatedAtActionWithProject()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                ProjectName = "New Project",
                OwnerId = 10
            };

            var createdProject = new ProjectResponseDto
            {
                ProjectId = 99,
                ProjectName = "New Project",
                OwnerId = 10
            };

            _projectServiceMock
                .Setup(s => s.CreateProjectAsync(dto))
                .ReturnsAsync(createdProject);

            // Act
            var result = await _controller.CreateProject(dto);

            // Assert
            var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.StatusCode.Should().Be(201);
            created.ActionName.Should().Be(nameof(ProjectController.GetProjectById));
            created.RouteValues!["id"].Should().Be(99);
            created.Value.Should().BeEquivalentTo(createdProject);
        }

        // =========================================================
        // PUT api/Project/{id}
        // =========================================================

        [Fact]
        public async Task UpdateProject_ValidDto_ReturnsOkWithUpdatedProject()
        {
            // Arrange
            var dto = new UpdateProjectDto { ProjectName = "Updated Name" };

            var updatedProject = new ProjectResponseDto
            {
                ProjectId = 1,
                ProjectName = "Updated Name",
                OwnerId = 10
            };

            _projectServiceMock
                .Setup(s => s.UpdateProjectAsync(1, dto))
                .ReturnsAsync(updatedProject);

            // Act
            var result = await _controller.UpdateProject(1, dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(updatedProject);
        }

        // =========================================================
        // DELETE api/Project/{id}
        // =========================================================

        [Fact]
        public async Task DeleteProject_ExistingId_ReturnsNoContent()
        {
            // Arrange
            _projectServiceMock
                .Setup(s => s.DeleteProjectAsync(1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteProject(1);

            // Assert
            result.Should().BeOfType<NoContentResult>()
                .Which.StatusCode.Should().Be(204);
        }
    }
}
