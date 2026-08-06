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

        public string Priority { get; set; } = string.Empty;

        public int ProjectId { get; set; }

        public int AssigneeId { get; set; }
    }
}
