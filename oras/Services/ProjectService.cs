using oras.DTOs.Project;
using oras.Exceptions;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Services.Interfaces;

namespace oras.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();

            return projects.Select(p => new ProjectResponseDto
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                OwnerId = p.OwnerId
            });
        }

        public async Task<ProjectResponseDto> GetProjectByIdAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
                throw new NotFoundException("Project not found.");

            return new ProjectResponseDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                OwnerId = project.OwnerId
            };
        }

        public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto createProjectDto)
        {
            var project = new Project
            {
                ProjectName = createProjectDto.ProjectName,
                OwnerId = createProjectDto.OwnerId
            };

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            return new ProjectResponseDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                OwnerId = project.OwnerId
            };
        }

        public async Task<ProjectResponseDto> UpdateProjectAsync(int id, UpdateProjectDto updateProjectDto)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
                throw new NotFoundException("Project not found.");

            project.ProjectName = updateProjectDto.ProjectName;

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();

            return new ProjectResponseDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                OwnerId = project.OwnerId
            };
        }

        public async Task DeleteProjectAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
                throw new NotFoundException("Project not found.");

            project.IsDeleted = true;

            await _projectRepository.DeleteAsync(project);
            await _projectRepository.SaveChangesAsync();
        }
    }
}