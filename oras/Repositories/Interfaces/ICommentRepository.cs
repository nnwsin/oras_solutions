using oras.Models;

namespace oras.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync();

        Task<Comment?> GetByIdAsync(int id);

        Task AddAsync(Comment comment);

        Task UpdateAsync(Comment comment);

        Task DeleteAsync(Comment comment);

        Task<IEnumerable<Comment>> GetByTaskIdAsync(int taskId);

        Task<IEnumerable<Comment>> GetByUserIdAsync(int userId);

        Task SaveChangesAsync();
    }
}
