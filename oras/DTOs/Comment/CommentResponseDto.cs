namespace oras.DTOs.Comment
{
    public class CommentResponseDto
    {
        public int CommentId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public int TaskId { get; set; }

        public string TaskTitle { get; set; } = string.Empty;

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;
    }
}
