using oras.DTOs.Tasks;
using oras.Enums;

namespace oras.Services.Interfaces
{
    public interface ITaskService
    {

        Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(
            int? projectId,
            AssignedTaskStatus? status,
            int? assigneeId);

        Task<TaskResponseDto> GetTaskByIdAsync(int id);

        Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto);

        Task<TaskResponseDto> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto);

        Task DeleteTaskAsync(int id);


    }
}
