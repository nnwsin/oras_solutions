using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using oras.Controllers;
using oras.DTOs.Comment;
using oras.Services.Interfaces;

namespace oras.Tests.Controllers
{
    /// <summary>
    /// Tests for CommentController — verifies correct HTTP response types and status codes.
    ///
    /// Why this class needs tests:
    ///   The controller determines the HTTP contract clients rely on.
    ///   These tests verify:
    ///     - GET returns 200 with the comment data
    ///     - POST returns 201 with CreatedAtAction pointing to GetCommentById
    ///     - PUT returns 200 with the updated comment
    ///     - DELETE returns 204 (No Content)
    ///
    /// Testing strategy:
    ///   - ICommentService is mocked — business logic is tested in CommentServiceTests
    ///   - One focused test per action verifying the HTTP response type and payload
    /// </summary>
    public class CommentControllerTests
    {
        private readonly Mock<ICommentService> _commentServiceMock;
        private readonly CommentController _controller;

        public CommentControllerTests()
        {
            _commentServiceMock = new Mock<ICommentService>();
            _controller = new CommentController(_commentServiceMock.Object);
        }

        // =========================================================
        // GET api/Comment
        // =========================================================

        [Fact]
        public async Task GetAllComments_ReturnsOkWithComments()
        {
            // Arrange
            var createdAt = DateTime.UtcNow.AddMinutes(-30);

            var comments = new List<CommentResponseDto>
            {
                new() { CommentId = 1, Content = "Looks good", CreatedAt = createdAt, TaskId = 5, UserId = 10 },
                new() { CommentId = 2, Content = "Needs revision", CreatedAt = createdAt, TaskId = 5, UserId = 20 }
            };

            _commentServiceMock
                .Setup(s => s.GetAllCommentsAsync())
                .ReturnsAsync(comments);

            // Act
            var result = await _controller.GetAllComments();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(comments);
        }

        [Fact]
        public async Task GetAllComments_WhenEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            _commentServiceMock
                .Setup(s => s.GetAllCommentsAsync())
                .ReturnsAsync(Enumerable.Empty<CommentResponseDto>());

            // Act
            var result = await _controller.GetAllComments();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            (ok.Value as IEnumerable<CommentResponseDto>).Should().BeEmpty();
        }

        // =========================================================
        // GET api/Comment/{id}
        // =========================================================

        [Fact]
        public async Task GetCommentById_ExistingComment_ReturnsOkWithComment()
        {
            // Arrange
            var createdAt = DateTime.UtcNow.AddMinutes(-10);

            var comment = new CommentResponseDto
            {
                CommentId = 1,
                Content = "Great work",
                CreatedAt = createdAt,
                TaskId = 5,
                UserId = 10
            };

            _commentServiceMock
                .Setup(s => s.GetCommentByIdAsync(1))
                .ReturnsAsync(comment);

            // Act
            var result = await _controller.GetCommentById(1);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(comment);
        }

        // =========================================================
        // POST api/Comment
        // =========================================================

        [Fact]
        public async Task CreateComment_ValidDto_ReturnsCreatedAtActionWithComment()
        {
            // Arrange
            var dto = new CreateCommentDto
            {
                Content = "New comment",
                TaskId = 5,
                UserId = 10
            };

            var createdAt = DateTime.UtcNow;

            var createdComment = new CommentResponseDto
            {
                CommentId = 77,
                Content = "New comment",
                CreatedAt = createdAt,
                TaskId = 5,
                UserId = 10
            };

            _commentServiceMock
                .Setup(s => s.CreateCommentAsync(dto))
                .ReturnsAsync(createdComment);

            // Act
            var result = await _controller.CreateComment(dto);

            // Assert
            var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.StatusCode.Should().Be(201);
            created.ActionName.Should().Be(nameof(CommentController.GetCommentById));
            created.RouteValues!["id"].Should().Be(77);
            created.Value.Should().BeEquivalentTo(createdComment);
        }

        // =========================================================
        // PUT api/Comment/{id}
        // =========================================================

        [Fact]
        public async Task UpdateComment_ValidDto_ReturnsOkWithUpdatedComment()
        {
            // Arrange
            var dto = new UpdateCommentDto { Content = "Updated content" };
            var createdAt = DateTime.UtcNow.AddMinutes(-30);

            var updatedComment = new CommentResponseDto
            {
                CommentId = 1,
                Content = "Updated content",
                CreatedAt = createdAt,
                TaskId = 5,
                UserId = 10
            };

            _commentServiceMock
                .Setup(s => s.UpdateCommentAsync(1, dto))
                .ReturnsAsync(updatedComment);

            // Act
            var result = await _controller.UpdateComment(1, dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(updatedComment);
        }

        // =========================================================
        // DELETE api/Comment/{id}
        // =========================================================

        [Fact]
        public async Task DeleteComment_ExistingId_ReturnsNoContent()
        {
            // Arrange
            _commentServiceMock
                .Setup(s => s.DeleteCommentAsync(1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteComment(1);

            // Assert
            result.Should().BeOfType<NoContentResult>()
                .Which.StatusCode.Should().Be(204);
        }
    }
}
