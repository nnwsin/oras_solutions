using oras.DTOs.Tasks;
using oras.Enums;
using oras.Exceptions;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Services.Interfaces;

namespace oras.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;

        public TaskService(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(
            int? projectId,
            AssignedTaskStatus? status,
            int? assigneeId)
        {
            var tasks = await _taskRepository.GetFilteredTasksAsync(
                projectId,
                status,
                assigneeId);

            return tasks.Select(t => new TaskResponseDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                DueDate = t.DueDate,
                Priority = t.Priority,
                ProjectId = t.ProjectId,
                AssigneeId = t.AssigneeId
            });
        }

        public async Task<TaskResponseDto> GetTaskByIdAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                throw new NotFoundException("Task not found.");

            return new TaskResponseDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                DueDate = task.DueDate,
                Priority = task.Priority,
                ProjectId = task.ProjectId,
                AssigneeId = task.AssigneeId
            };
        }

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto)
        {
            var project = await _projectRepository.GetByIdAsync(createTaskDto.ProjectId);

            if (project == null)
                throw new NotFoundException("Project not found.");

            var user = await _userRepository.GetByIdAsync(createTaskDto.AssigneeId);

            if (user == null)
                throw new NotFoundException("User not found.");

            var task = new AssignedTask
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Status = createTaskDto.Status,
                DueDate = createTaskDto.DueDate,
                Priority = createTaskDto.Priority,
                ProjectId = createTaskDto.ProjectId,
                AssigneeId = createTaskDto.AssigneeId
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            return new TaskResponseDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                DueDate = task.DueDate,
                Priority = task.Priority,
                ProjectId = task.ProjectId,
                AssigneeId = task.AssigneeId
            };
        }

        public async Task<TaskResponseDto> UpdateTaskAsync(
            int id,
            UpdateTaskDto updateTaskDto)
        {
            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                throw new NotFoundException("Task not found.");

            task.Title = updateTaskDto.Title;
            task.Description = updateTaskDto.Description;
            task.Status = updateTaskDto.Status;
            task.DueDate = updateTaskDto.DueDate;
            task.Priority = updateTaskDto.Priority;

            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveChangesAsync();

            return new TaskResponseDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                DueDate = task.DueDate,
                Priority = task.Priority,
                ProjectId = task.ProjectId,
                AssigneeId = task.AssigneeId
            };
        }

        public async Task DeleteTaskAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                throw new NotFoundException("Task not found.");

            task.IsDeleted = true;

            await _taskRepository.SaveChangesAsync();
        }
    }
}