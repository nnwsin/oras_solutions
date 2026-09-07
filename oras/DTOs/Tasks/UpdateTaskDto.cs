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
        public Priority Priority { get; set; } = Priority.Medium;

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int AssigneeId { get; set; }
    }
}
