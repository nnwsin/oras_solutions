using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using oras.DTOs.Comment;
using oras.Services.Interfaces;

namespace oras.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: api/Comment
        // GET: api/Comment?taskId=1
        [HttpGet]
        public async Task<IActionResult> GetAllComments([FromQuery] int? taskId)
        {
            var comments = await _commentService.GetAllCommentsAsync(taskId);
            return Ok(comments);
        }

        // GET: api/Comment/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            return Ok(comment);
        }

        // POST: api/Comment
        [HttpPost]
        public async Task<IActionResult> CreateComment(CreateCommentDto createCommentDto)
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                           ?? User?.FindFirst("sub");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int authenticatedUserId))
            {
                createCommentDto.UserId = authenticatedUserId;
            }

            var comment = await _commentService.CreateCommentAsync(createCommentDto);

            return CreatedAtAction(
                nameof(GetCommentById),
                new { id = comment.CommentId },
                comment);
        }

        // PUT: api/Comment/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(
            int id,
            UpdateCommentDto updateCommentDto)
        {
            var comment = await _commentService.UpdateCommentAsync(id, updateCommentDto);

            return Ok(comment);
        }

        // DELETE: api/Comment/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            await _commentService.DeleteCommentAsync(id);

            return NoContent();
        }
    }
}