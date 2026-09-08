using Microsoft.EntityFrameworkCore;
using oras.Data;
using oras.Models;
using oras.Repositories.Interfaces;

namespace oras.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
