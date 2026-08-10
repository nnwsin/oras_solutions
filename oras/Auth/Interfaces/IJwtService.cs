using oras.Models;

namespace oras.Auth.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
