using System.ComponentModel.DataAnnotations;

namespace oras.DTOs.Project
{
    public class UpdateProjectDto
    {


        [Required]
        [MaxLength(100)]
        public string ProjectName { get; set; } = string.Empty;
    }
}
