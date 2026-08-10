using System.ComponentModel.DataAnnotations;

namespace oras.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        public int TaskId { get; set; }

        public int UserId { get; set; }



        [Required]
        [MaxLength(500)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



        
        public bool IsDeleted { get; set; } = false;
        
        
        // navigation
        public AssignedTask Task { get; set; } = null!;

        public User User { get; set; } = null!;


    }
}
