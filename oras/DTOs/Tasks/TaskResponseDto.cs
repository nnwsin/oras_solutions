using oras.Enums;

namespace oras.DTOs.Tasks
{
    public class TaskResponseDto
    {

        public int TaskId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public AssignedTaskStatus Status { get; set; }

        public DateTime DueDate { get; set; }

        public Priority Priority { get; set; } = Priority.Medium;

        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public int AssigneeId { get; set; }

        public string AssigneeName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
