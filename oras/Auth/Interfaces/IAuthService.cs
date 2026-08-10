using oras.Auth.Dto;

namespace oras.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        Task RegisterAsync(RegisterDto registerDto);
    }
}