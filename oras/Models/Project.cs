using System.ComponentModel.DataAnnotations;

namespace oras.Models
{
    public class Project
    {

        public int ProjectId { get; set; }


        [Required]
        [MaxLength(100)]
        public string ProjectName { get; set; } = string.Empty;

        public int OwnerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;


        // navigation 

        public User Owner { get; set; } = null!;

        public ICollection<AssignedTask> Tasks { get; set; } = new List<AssignedTask>();
    }
}
