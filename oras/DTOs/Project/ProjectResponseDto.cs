namespace oras.DTOs.Project
{
    public class ProjectResponseDto
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public int OwnerId { get; set; }

        public string OwnerName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
