using System.ComponentModel.DataAnnotations;

namespace oras.Models
{
    public class User
    {
        public int UserId { get; set; }


        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;


        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;


        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public bool IsDeleted { get; set; } = false;

        // navigation 
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        public ICollection<AssignedTask> AssignedTasks { get; set; } = new List<AssignedTask>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();


    }
}
