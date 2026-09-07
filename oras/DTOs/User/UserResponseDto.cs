using oras.Enums;

namespace oras.DTOs.User
{
    public class UserResponseDto
    {
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
