using oras.Models;

namespace oras.Repositories.Interfaces
{
    public interface IProjectRepository
    {

        Task<IEnumerable<Project>> GetAllAsync();

        Task<Project?> GetByIdAsync(int id);

        Task AddAsync(Project project);

        Task UpdateAsync(Project project);

        Task DeleteAsync(Project project);

        Task SaveChangesAsync();
    }
}
