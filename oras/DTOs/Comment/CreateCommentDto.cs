using System.ComponentModel.DataAnnotations;

namespace oras.DTOs.Comment
{
    public class CreateCommentDto
    {
        [Required]
        [MaxLength(500)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
