using System.ComponentModel.DataAnnotations;

namespace oras.DTOs.Comment
{
    public class UpdateCommentDto
    {
        [Required]
        [MaxLength(500)]
        public string Content { get; set; } = string.Empty;
    }
}
