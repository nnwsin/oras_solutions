using oras.DTOs.User;
using oras.Exceptions;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Services.Interfaces;

namespace oras.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ICommentRepository _commentRepository;

        public UserService(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IProjectRepository projectRepository,
            ITaskRepository taskRepository,
            ICommentRepository commentRepository)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _commentRepository = commentRepository;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            });
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new NotFoundException("User not found.");

            return new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);

            if (existingUser != null)
                throw new BadRequestException("Email already exists.");

            var user = new User
            {
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                Role = createUserDto.Role,
                Password = _passwordService.HashPassword(createUserDto.Password)
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new NotFoundException("User not found.");

            var existingUser = await _userRepository.GetByEmailAsync(updateUserDto.Email);

            if (existingUser != null && existingUser.UserId != id)
                throw new BadRequestException("Email already exists.");

            user.Name = updateUserDto.Name;
            user.Email = updateUserDto.Email;
            user.Role = updateUserDto.Role;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new NotFoundException("User not found.");

            user.IsDeleted = true;

            var projects = await _projectRepository.GetByOwnerIdAsync(id);
            foreach (var project in projects)
            {
                project.IsDeleted = true;
                var projTasks = await _taskRepository.GetFilteredTasksAsync(project.ProjectId, null, null);
                foreach (var task in projTasks)
                {
                    task.IsDeleted = true;
                    var taskComments = await _commentRepository.GetByTaskIdAsync(task.TaskId);
                    foreach (var c in taskComments) c.IsDeleted = true;
                }
            }

            var assignedTasks = await _taskRepository.GetFilteredTasksAsync(null, null, id);
            foreach (var task in assignedTasks)
            {
                task.IsDeleted = true;
                var taskComments = await _commentRepository.GetByTaskIdAsync(task.TaskId);
                foreach (var c in taskComments) c.IsDeleted = true;
            }

            var comments = await _commentRepository.GetByUserIdAsync(id);
            foreach (var comment in comments)
            {
                comment.IsDeleted = true;
            }

            await _userRepository.SaveChangesAsync();
        }
    }
}
