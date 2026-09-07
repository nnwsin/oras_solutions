using System.ComponentModel.DataAnnotations;
using oras.Enums; // Add this

namespace oras.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Employee; // Add this

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<AssignedTask> AssignedTasks { get; set; } = new List<AssignedTask>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
