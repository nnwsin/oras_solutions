using oras.DTOs.User;

namespace oras.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();

        Task<UserResponseDto> GetUserByIdAsync(int id);

        Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);

        Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);

        Task DeleteUserAsync(int id);
    }
}