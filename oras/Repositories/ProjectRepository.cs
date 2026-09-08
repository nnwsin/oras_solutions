using Microsoft.EntityFrameworkCore;
using oras.Data;
using oras.Models;
using oras.Repositories.Interfaces;

namespace oras.Repositories
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _dbSet.Include(p => p.Owner).ToListAsync();
        }

        public override async Task<Project?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.ProjectId == id);
        }

        public async Task<IEnumerable<Project>> GetByOwnerIdAsync(int ownerId)
        {
            return await _dbSet.Include(p => p.Owner).Where(p => p.OwnerId == ownerId).ToListAsync();
        }
    }
}
