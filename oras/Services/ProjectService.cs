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
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ICommentRepository _commentRepository;

        public ProjectService(
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            ITaskRepository taskRepository,
            ICommentRepository commentRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _taskRepository = taskRepository;
            _commentRepository = commentRepository;
        }

        public async Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();

            return projects.Select(p => new ProjectResponseDto
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner?.Name ?? string.Empty,
                CreatedAt = p.CreatedAt
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
                OwnerId = project.OwnerId,
                OwnerName = project.Owner?.Name ?? string.Empty,
                CreatedAt = project.CreatedAt
            };
        }

        public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto createProjectDto)
        {
            var owner = await _userRepository.GetByIdAsync(createProjectDto.OwnerId);

            if (owner == null)
                throw new NotFoundException("Owner not found.");

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
                OwnerId = project.OwnerId,
                OwnerName = owner.Name,
                CreatedAt = project.CreatedAt
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
                OwnerId = project.OwnerId,
                OwnerName = project.Owner?.Name ?? string.Empty,
                CreatedAt = project.CreatedAt
            };
        }

        public async Task DeleteProjectAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
                throw new NotFoundException("Project not found.");

            project.IsDeleted = true;

            var tasks = await _taskRepository.GetFilteredTasksAsync(id, null, null);
            foreach (var task in tasks)
            {
                task.IsDeleted = true;
                var comments = await _commentRepository.GetByTaskIdAsync(task.TaskId);
                foreach (var comment in comments)
                {
                    comment.IsDeleted = true;
                }
            }

            await _projectRepository.SaveChangesAsync();
        }
    }
}