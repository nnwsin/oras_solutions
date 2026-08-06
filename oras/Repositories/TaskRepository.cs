using Microsoft.EntityFrameworkCore;
using oras.Data;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Enums;
namespace oras.Repositories
{
    public class TaskRepository : ITaskRepository
    {

        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssignedTask>> GetAllAsync()
        {
            return await _context.Tasks.ToListAsync();
        }

        public async Task<AssignedTask?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskId == id);
        }

        public async Task AddAsync(AssignedTask task)
        {
            await _context.Tasks.AddAsync(task);
        }

        public Task UpdateAsync(AssignedTask task)
        {
            _context.Tasks.Update(task);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(AssignedTask task)
        {
            _context.Tasks.Update(task);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }



        public async Task<IEnumerable<AssignedTask>> GetFilteredTasksAsync(int? projectId, AssignedTaskStatus? status,int? assigneeId)
        {
            var query = _context.Tasks.AsQueryable();

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (assigneeId.HasValue)
            {
                query = query.Where(t => t.AssigneeId == assigneeId.Value);
            }

            return await query.ToListAsync();
        }

    }
}
