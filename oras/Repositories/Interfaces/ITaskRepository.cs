using oras.Models;

namespace oras.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<AssignedTask>> GetAllAsync();

        Task<AssignedTask?> GetByIdAsync(int id);

        Task AddAsync(AssignedTask task);

        Task UpdateAsync(AssignedTask task);

        Task DeleteAsync(AssignedTask task);

        Task SaveChangesAsync();
    }
}
