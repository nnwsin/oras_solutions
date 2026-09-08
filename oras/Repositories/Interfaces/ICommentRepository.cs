using oras.Models;

namespace oras.Repositories.Interfaces
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetByTaskIdAsync(int taskId);

        Task<IEnumerable<Comment>> GetByUserIdAsync(int userId);
    }
}
