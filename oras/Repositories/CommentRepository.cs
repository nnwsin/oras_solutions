using Microsoft.EntityFrameworkCore;
using oras.Data;
using oras.Models;
using oras.Repositories.Interfaces;

namespace oras.Repositories
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        public CommentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await _dbSet.Include(c => c.Task).Include(c => c.User).ToListAsync();
        }

        public override async Task<Comment?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Task)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == id);
        }

        public async Task<IEnumerable<Comment>> GetByTaskIdAsync(int taskId)
        {
            return await _dbSet.Include(c => c.Task).Include(c => c.User).Where(c => c.TaskId == taskId).ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetByUserIdAsync(int userId)
        {
            return await _dbSet.Include(c => c.Task).Include(c => c.User).Where(c => c.UserId == userId).ToListAsync();
        }
    }
}
