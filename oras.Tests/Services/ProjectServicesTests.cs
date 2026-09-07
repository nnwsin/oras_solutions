using Moq;
using oras.DTOs.Project;
using oras.Exceptions;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Services;

namespace oras.Tests.Services
{
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        private readonly ProjectService _projectService;

        public ProjectServiceTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _projectService = new ProjectService(
                _projectRepositoryMock.Object,
                _userRepositoryMock.Object
            );
        }


        // =========================================================
        // GetAllProjectsAsync
        // =========================================================

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsMappedProjects()
        {
            // Arrange

            var projects = new List<Project>
            {
                new Project
                {
                    ProjectId = 1,
                    ProjectName = "Project One",
                    OwnerId = 10
                },
                new Project
                {
                    ProjectId = 2,
                    ProjectName = "Project Two",
                    OwnerId = 20
                }
            };

            _projectRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(projects);


            // Act

            var result = await _projectService.GetAllProjectsAsync();


            // Assert

            Assert.NotNull(result);

            var resultList = result.ToList();

            Assert.Equal(2, resultList.Count);

            Assert.Equal(1, resultList[0].ProjectId);
            Assert.Equal("Project One", resultList[0].ProjectName);
            Assert.Equal(10, resultList[0].OwnerId);

            Assert.Equal(2, resultList[1].ProjectId);
            Assert.Equal("Project Two", resultList[1].ProjectName);
            Assert.Equal(20, resultList[1].OwnerId);

            _projectRepositoryMock.Verify(
                x => x.GetAllAsync(),
                Times.Once
            );
        }


        [Fact]
        public async Task GetAllProjectsAsync_WhenNoProjects_ReturnsEmptyCollection()
        {
            // Arrange

            var projects = new List<Project>();

            _projectRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(projects);


            // Act

            var result = await _projectService.GetAllProjectsAsync();


            // Assert

            Assert.NotNull(result);
            Assert.Empty(result);

            _projectRepositoryMock.Verify(
                x => x.GetAllAsync(),
                Times.Once
            );
        }


        // =========================================================
        // GetProjectByIdAsync
        // =========================================================

        [Fact]
        public async Task GetProjectByIdAsync_WhenProjectExists_ReturnsProject()
        {
            // Arrange

            var project = new Project
            {
                ProjectId = 1,
                ProjectName = "Project One",
                OwnerId = 10
            };

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);


            // Act

            var result = await _projectService.GetProjectByIdAsync(1);


            // Assert

            Assert.NotNull(result);

            Assert.Equal(1, result.ProjectId);
            Assert.Equal("Project One", result.ProjectName);
            Assert.Equal(10, result.OwnerId);

            _projectRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );
        }


        [Fact]
        public async Task GetProjectByIdAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Project?)null);


            // Act & Assert

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _projectService.GetProjectByIdAsync(1)
            );

            Assert.Equal("Project not found.", exception.Message);

            _projectRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );
        }


        // =========================================================
        // CreateProjectAsync
        // =========================================================

        [Fact]
        public async Task CreateProjectAsync_WhenOwnerExists_CreatesProject()
        {
            // Arrange

            var createDto = new CreateProjectDto
            {
                ProjectName = "New Project",
                OwnerId = 10
            };

            var owner = new User();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(owner);

            _projectRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _projectRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);


            // Act

            var result = await _projectService.CreateProjectAsync(createDto);


            // Assert

            Assert.NotNull(result);

            Assert.Equal("New Project", result.ProjectName);
            Assert.Equal(10, result.OwnerId);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once
            );

            _projectRepositoryMock.Verify(
                x => x.AddAsync(It.Is<Project>(p =>
                    p.ProjectName == "New Project" &&
                    p.OwnerId == 10
                )),
                Times.Once
            );

            _projectRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }


        [Fact]
        public async Task CreateProjectAsync_WhenOwnerDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange

            var createDto = new CreateProjectDto
            {
                ProjectName = "New Project",
                OwnerId = 10
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync((User?)null);


            // Act & Assert

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _projectService.CreateProjectAsync(createDto)
            );

            Assert.Equal("Owner not found.", exception.Message);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once
            );

            _projectRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Project>()),
                Times.Never
            );

            _projectRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        // =========================================================
        // UpdateProjectAsync
        // =========================================================

        [Fact]
        public async Task UpdateProjectAsync_WhenProjectExists_UpdatesProject()
        {
            // Arrange

            var project = new Project
            {
                ProjectId = 1,
                ProjectName = "Old Name",
                OwnerId = 10
            };

            var updateDto = new UpdateProjectDto
            {
                ProjectName = "Updated Name"
            };

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);

            _projectRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _projectRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);


            // Act

            var result = await _projectService.UpdateProjectAsync(
                1,
                updateDto
            );


            // Assert

            Assert.NotNull(result);

            Assert.Equal(1, result.ProjectId);
            Assert.Equal("Updated Name", result.ProjectName);
            Assert.Equal(10, result.OwnerId);

            _projectRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _projectRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<Project>(p =>
                    p.ProjectId == 1 &&
                    p.ProjectName == "Updated Name" &&
                    p.OwnerId == 10
                )),
                Times.Once
            );

            _projectRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }


        [Fact]
        public async Task UpdateProjectAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange

            var updateDto = new UpdateProjectDto
            {
                ProjectName = "Updated Name"
            };

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Project?)null);


            // Act & Assert

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _projectService.UpdateProjectAsync(1, updateDto)
            );

            Assert.Equal("Project not found.", exception.Message);

            _projectRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _projectRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Project>()),
                Times.Never
            );

            _projectRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        // =========================================================
        // DeleteProjectAsync
        // =========================================================

        [Fact]
        public async Task DeleteProjectAsync_WhenProjectExists_SoftDeletesProject()
        {
            // Arrange

            var project = new Project
            {
                ProjectId = 1,
                ProjectName = "Project One",
                OwnerId = 10,
                IsDeleted = false
            };

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);

            _projectRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);


            // Act

            await _projectService.DeleteProjectAsync(1);


            // Assert

            Assert.True(project.IsDeleted);

            _projectRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _projectRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }


        [Fact]
        public async Task DeleteProjectAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Project?)null);


            // Act & Assert

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _projectService.DeleteProjectAsync(1)
            );

            Assert.Equal("Project not found.", exception.Message);

            _projectRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _projectRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }
    }
}