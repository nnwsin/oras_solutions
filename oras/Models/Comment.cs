using System.ComponentModel.DataAnnotations;

namespace oras.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        public int TaskId { get; set; }

        public int UserId { get; set; }


        [Required]
        public string Discussion { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



        
        public bool IsDeleted { get; set; } = false;
        
        
        // navigatiojn prop
        public AssignedTask Task { get; set; } = null!;

        public User User { get; set; } = null!;


    }
}
