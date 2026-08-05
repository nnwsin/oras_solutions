using oras.DTOs.Project;

namespace oras.Services.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync();

        Task<ProjectResponseDto> GetProjectByIdAsync(int id);

        Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto createProjectDto);

        Task<ProjectResponseDto> UpdateProjectAsync(int id, UpdateProjectDto updateProjectDto);

        Task DeleteProjectAsync(int id);
    }
}