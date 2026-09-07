using oras.DTOs.Comment;
using oras.Exceptions;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Services.Interfaces;

namespace oras.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public CommentService(
            ICommentRepository commentRepository,
            ITaskRepository taskRepository,
            IUserRepository userRepository)
        {
            _commentRepository = commentRepository;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<CommentResponseDto>> GetAllCommentsAsync(int? taskId = null)
        {
            var comments = taskId.HasValue
                ? await _commentRepository.GetByTaskIdAsync(taskId.Value)
                : await _commentRepository.GetAllAsync();

            return comments.Select(c => new CommentResponseDto
            {
                CommentId = c.CommentId,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                TaskId = c.TaskId,
                TaskTitle = c.Task?.Title ?? string.Empty,
                UserId = c.UserId,
                UserName = c.User?.Name ?? string.Empty
            });
        }

        public async Task<CommentResponseDto> GetCommentByIdAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
                throw new NotFoundException("Comment not found.");

            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                TaskTitle = comment.Task?.Title ?? string.Empty,
                UserId = comment.UserId,
                UserName = comment.User?.Name ?? string.Empty
            };
        }

        public async Task<CommentResponseDto> CreateCommentAsync(CreateCommentDto createCommentDto)
        {
            var task = await _taskRepository.GetByIdAsync(createCommentDto.TaskId);

            if (task == null)
                throw new NotFoundException("Task not found.");

            var user = await _userRepository.GetByIdAsync(createCommentDto.UserId);

            if (user == null)
                throw new NotFoundException("User not found.");

            var comment = new Comment
            {
                Content = createCommentDto.Content,
                TaskId = createCommentDto.TaskId,
                UserId = createCommentDto.UserId
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();

            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                TaskTitle = task.Title,
                UserId = comment.UserId,
                UserName = user.Name
            };
        }

        public async Task<CommentResponseDto> UpdateCommentAsync(
            int id,
            UpdateCommentDto updateCommentDto)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
                throw new NotFoundException("Comment not found.");

            comment.Content = updateCommentDto.Content;

            await _commentRepository.UpdateAsync(comment);
            await _commentRepository.SaveChangesAsync();

            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                TaskTitle = comment.Task?.Title ?? string.Empty,
                UserId = comment.UserId,
                UserName = comment.User?.Name ?? string.Empty
            };
        }

        public async Task DeleteCommentAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
                throw new NotFoundException("Comment not found.");

            comment.IsDeleted = true;

            await _commentRepository.SaveChangesAsync();
        }
    }
}