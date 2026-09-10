using Microsoft.EntityFrameworkCore;
using oras.Data;
using oras.Enums;
using oras.Models;
using oras.Repositories.Interfaces;

namespace oras.Repositories
{
    public class TaskRepository : GenericRepository<AssignedTask>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<AssignedTask>> GetAllAsync()
        {
            return await _dbSet
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public override async Task<AssignedTask?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.TaskId == id);
        }

        public async Task<IEnumerable<AssignedTask>> GetFilteredTasksAsync(int? projectId, AssignedTaskStatus? status, int? assigneeId)
        {
            var query = _dbSet
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .AsQueryable();

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

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }
    }
}
