using oras.Enums;
using System.ComponentModel.DataAnnotations;

namespace oras.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public AssignedTaskStatus Status { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = string.Empty;
                                                                
    }
}
