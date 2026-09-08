using oras.Enums;
using oras.Models;

namespace oras.Repositories.Interfaces
{
    public interface ITaskRepository : IGenericRepository<AssignedTask>
    {
        Task<IEnumerable<AssignedTask>> GetFilteredTasksAsync(
            int? projectId,
            AssignedTaskStatus? status,
            int? assigneeId);
    }
}
