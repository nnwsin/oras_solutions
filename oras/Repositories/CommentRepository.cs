using Microsoft.EntityFrameworkCore;
using oras.Data;
using oras.Models;
using oras.Repositories.Interfaces;

namespace oras.Repositories
{
    public class CommentRepository : ICommentRepository
    {

        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await _context.Comments.ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == id);
        }

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }

        public Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
