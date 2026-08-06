using oras.DTOs.Comment;

namespace oras.Services.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentResponseDto>> GetAllCommentsAsync();

        Task<CommentResponseDto> GetCommentByIdAsync(int id);

        Task<CommentResponseDto> CreateCommentAsync(CreateCommentDto createCommentDto);

        Task<CommentResponseDto> UpdateCommentAsync(int id, UpdateCommentDto updateCommentDto);

        Task DeleteCommentAsync(int id);
    }
}
