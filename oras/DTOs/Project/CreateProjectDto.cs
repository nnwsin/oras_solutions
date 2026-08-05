using System.ComponentModel.DataAnnotations;

namespace oras.DTOs.Project
{
    public class CreateProjectDto
    {
        [Required]
        [MaxLength(100)]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        public int OwnerId { get; set; }
    }
}
