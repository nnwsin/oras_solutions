using oras.Models;

namespace oras.Repositories.Interfaces
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<IEnumerable<Project>> GetByOwnerIdAsync(int ownerId);
    }
}
