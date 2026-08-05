using System.ComponentModel.DataAnnotations;

namespace oras.Models
{
    public class AssignedTask
    {

        [Key]
        public int TaskId { get; set; }

        public int ProjectId { get; set; }



        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;


        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;



        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = string.Empty;


        public int AssigneeId { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;


        // navigation 


        public Project Project { get; set; } = null!;

        public User Assignee { get; set; } = null!;


        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
