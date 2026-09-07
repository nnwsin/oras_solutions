using FluentAssertions;
using Moq;
using oras.DTOs.Comment;
using oras.Exceptions;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Services;

namespace oras.Tests.Services
{
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            _commentRepositoryMock = new Mock<ICommentRepository>();
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _commentService = new CommentService(
                _commentRepositoryMock.Object,
                _taskRepositoryMock.Object,
                _userRepositoryMock.Object
            );
        }


        // =========================================================
        // GetAllCommentsAsync
        // /////////////////////

        [Fact]
        public async Task GetAllCommentsAsync_WhenCommentsExist_ReturnsMappedComments()
        {
            // Arrange

            var createdAt1 = DateTime.UtcNow.AddMinutes(-10);
            var createdAt2 = DateTime.UtcNow.AddMinutes(-5);

            var comments = new List<Comment>
            {
                new Comment
                {
                    CommentId = 1,
                    Content = "Comment 1",
                    CreatedAt = createdAt1,
                    TaskId = 1,
                    UserId = 1
                },
                new Comment
                {
                    CommentId = 2,
                    Content = "Comment 2",
                    CreatedAt = createdAt2,
                    TaskId = 1,
                    UserId = 2
                }
            };

            _commentRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(comments);


            // Act

            var result = await _commentService.GetAllCommentsAsync();


            // Assert

            result.Should().NotBeNull();

            result.Should().BeEquivalentTo(
                new[]
                {
                    new CommentResponseDto
                    {
                        CommentId = 1,
                        Content = "Comment 1",
                        CreatedAt = createdAt1,
                        TaskId = 1,
                        UserId = 1
                    },
                    new CommentResponseDto
                    {
                        CommentId = 2,
                        Content = "Comment 2",
                        CreatedAt = createdAt2,
                        TaskId = 1,
                        UserId = 2
                    }
                }
            );

            _commentRepositoryMock.Verify(
                x => x.GetAllAsync(),
                Times.Once
            );
        }


        [Fact]
        public async Task GetAllCommentsAsync_WhenNoCommentsExist_ReturnsEmptyCollection()
        {
            // Arrange

            var comments = new List<Comment>();

            _commentRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(comments);


            // Act

            var result = await _commentService.GetAllCommentsAsync();


            // Assert

            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _commentRepositoryMock.Verify(
                x => x.GetAllAsync(),
                Times.Once
            );
        }


        [Fact]
        public async Task GetAllCommentsAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange

            _commentRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception("Database error"));


            // Act

            var act = () => _commentService.GetAllCommentsAsync();


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Database error");

            _commentRepositoryMock.Verify(
                x => x.GetAllAsync(),
                Times.Once
            );
        }






        //////////////////////////////////
        ///////////////////////////////////
        // GET by Id cooments 


        [Fact]
        public async Task GetCommentByIdAsync_WhenCommentExists_ReturnsMappedComment()
        {
            // Arrange

            var createdAt = DateTime.UtcNow.AddMinutes(-10);

            var comment = new Comment
            {
                CommentId = 1,
                Content = "Test comment",
                CreatedAt = createdAt,
                TaskId = 10,
                UserId = 20
            };

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(comment);


            // Act

            var result = await _commentService.GetCommentByIdAsync(1);


            // Assert

            result.Should().NotBeNull();

            result.Should().BeEquivalentTo(
                new CommentResponseDto
                {
                    CommentId = 1,
                    Content = "Test comment",
                    CreatedAt = createdAt,
                    TaskId = 10,
                    UserId = 20
                }
            );

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );
        }


        [Fact]
        public async Task GetCommentByIdAsync_WhenCommentDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Comment?)null);


            // Act

            var act = () => _commentService.GetCommentByIdAsync(1);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<NotFoundException>();

            exception.Which.Message.Should()
                .Be("Comment not found.");

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );
        }


        [Fact]
        public async Task GetCommentByIdAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));


            // Act

            var act = () => _commentService.GetCommentByIdAsync(1);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Database error");

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );
        }



        //////////////////////////////////
        //////////////////////////////////
        ///

        // 3 create user



        // =========================================================
        // CreateCommentAsync
        // =========================================================

        [Fact]
        public async Task CreateCommentAsync_WhenTaskAndUserExist_CreatesComment()
        {
            // Arrange

            var createDto = new CreateCommentDto
            {
                Content = "This is a test comment",
                TaskId = 10,
                UserId = 20
            };

            var task = new AssignedTask
            {
                TaskId = 10
            };

            var user = new User
            {
                UserId = 20
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(task);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(20))
                .ReturnsAsync(user);

            _commentRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Comment>()))
                .Returns(Task.CompletedTask);

            _commentRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);


            // Act

            var result = await _commentService.CreateCommentAsync(createDto);


            // Assert

            result.Should().NotBeNull();

            result.Content.Should().Be("This is a test comment");
            result.TaskId.Should().Be(10);
            result.UserId.Should().Be(20);

            _taskRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once
            );

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(20),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.AddAsync(It.Is<Comment>(c =>
                    c.Content == "This is a test comment" &&
                    c.TaskId == 10 &&
                    c.UserId == 20
                )),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }


        [Fact]
        public async Task CreateCommentAsync_WhenTaskDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange

            var createDto = new CreateCommentDto
            {
                Content = "This is a test comment",
                TaskId = 10,
                UserId = 20
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync((AssignedTask?)null);


            // Act

            var act = () => _commentService.CreateCommentAsync(createDto);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<NotFoundException>();

            exception.Which.Message.Should()
                .Be("Task not found.");

            _taskRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once
            );

            // Because task doesn't exist,
            // user should never be checked.

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(20),
                Times.Never
            );

            // Comment should never be created.

            _commentRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Comment>()),
                Times.Never
            );

            // Save should never happen.

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        [Fact]
        public async Task CreateCommentAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange

            var createDto = new CreateCommentDto
            {
                Content = "This is a test comment",
                TaskId = 10,
                UserId = 20
            };

            var task = new AssignedTask
            {
                TaskId = 10
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(task);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(20))
                .ReturnsAsync((User?)null);


            // Act

            var act = () => _commentService.CreateCommentAsync(createDto);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<NotFoundException>();

            exception.Which.Message.Should()
                .Be("User not found.");

            _taskRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once
            );

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(20),
                Times.Once
            );

            // Comment should never be created.

            _commentRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Comment>()),
                Times.Never
            );

            // Save should never happen.

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        [Fact]
        public async Task CreateCommentAsync_WhenTaskRepositoryThrows_PropagatesException()
        {
            // Arrange

            var createDto = new CreateCommentDto
            {
                Content = "This is a test comment",
                TaskId = 10,
                UserId = 20
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ThrowsAsync(new Exception("Database error"));


            // Act

            var act = () => _commentService.CreateCommentAsync(createDto);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Database error");

            _taskRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once
            );

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(20),
                Times.Never
            );

            _commentRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Comment>()),
                Times.Never
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        [Fact]
        public async Task CreateCommentAsync_WhenUserRepositoryThrows_PropagatesException()
        {
            // Arrange

            var createDto = new CreateCommentDto
            {
                Content = "This is a test comment",
                TaskId = 10,
                UserId = 20
            };

            var task = new AssignedTask
            {
                TaskId = 10
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(task);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(20))
                .ThrowsAsync(new Exception("Database error"));


            // Act

            var act = () => _commentService.CreateCommentAsync(createDto);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Database error");

            _taskRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once
            );

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(20),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Comment>()),
                Times.Never
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        [Fact]
        public async Task CreateCommentAsync_WhenAddCommentFails_PropagatesException()
        {
            // Arrange

            var createDto = new CreateCommentDto
            {
                Content = "This is a test comment",
                TaskId = 10,
                UserId = 20
            };

            var task = new AssignedTask
            {
                TaskId = 10
            };

            var user = new User
            {
                UserId = 20
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(task);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(20))
                .ReturnsAsync(user);

            _commentRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Comment>()))
                .ThrowsAsync(new Exception("Database error"));


            // Act

            var act = () => _commentService.CreateCommentAsync(createDto);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Database error");

            _taskRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once
            );

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(20),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Comment>()),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        [Fact]
        public async Task CreateCommentAsync_WhenSaveChangesFails_PropagatesException()
        {
            // Arrange

            var createDto = new CreateCommentDto
            {
                Content = "This is a test comment",
                TaskId = 10,
                UserId = 20
            };

            var task = new AssignedTask
            {
                TaskId = 10
            };

            var user = new User
            {
                UserId = 20
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(task);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(20))
                .ReturnsAsync(user);

            _commentRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Comment>()))
                .Returns(Task.CompletedTask);

            _commentRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database error"));


            // Act

            var act = () => _commentService.CreateCommentAsync(createDto);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Database error");

            _taskRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once
            );

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(20),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Comment>()),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }



        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        ///////////////////////////////////////////////////
        /// 4 update one Update one
        /// 


        // =========================================================
        // UpdateCommentAsync
        // =========================================================

        [Fact]
        public async Task UpdateCommentAsync_WhenCommentExists_UpdatesAndReturnsComment()
        {
            // Arrange

            var createdAt = DateTime.UtcNow.AddMinutes(-10);

            var comment = new Comment
            {
                CommentId = 1,
                Content = "Old comment",
                CreatedAt = createdAt,
                TaskId = 10,
                UserId = 20
            };

            var updateDto = new UpdateCommentDto
            {
                Content = "Updated comment"
            };

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(comment);

            _commentRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Comment>()))
                .Returns(Task.CompletedTask);

            _commentRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);


            // Act

            var result = await _commentService.UpdateCommentAsync(
                1,
                updateDto
            );


            // Assert

            result.Should().NotBeNull();

            result.Should().BeEquivalentTo(
                new CommentResponseDto
                {
                    CommentId = 1,
                    Content = "Updated comment",
                    CreatedAt = createdAt,
                    TaskId = 10,
                    UserId = 20
                }
            );

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<Comment>(c =>
                    c.CommentId == 1 &&
                    c.Content == "Updated comment" &&
                    c.CreatedAt == createdAt &&
                    c.TaskId == 10 &&
                    c.UserId == 20
                )),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }


        [Fact]
        public async Task UpdateCommentAsync_WhenCommentDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange

            var updateDto = new UpdateCommentDto
            {
                Content = "Updated comment"
            };

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Comment?)null);


            // Act

            var act = () => _commentService.UpdateCommentAsync(
                1,
                updateDto
            );


            // Assert

            var exception = await act.Should()
                .ThrowAsync<NotFoundException>();

            exception.Which.Message.Should()
                .Be("Comment not found.");

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Comment>()),
                Times.Never
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        [Fact]
        public async Task UpdateCommentAsync_WhenGetCommentFails_PropagatesException()
        {
            // Arrange

            var updateDto = new UpdateCommentDto
            {
                Content = "Updated comment"
            };

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));


            // Act

            var act = () => _commentService.UpdateCommentAsync(
                1,
                updateDto
            );


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Database error");

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Comment>()),
                Times.Never
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        [Fact]
        public async Task UpdateCommentAsync_WhenUpdateFails_PropagatesException()
        {
            // Arrange

            var comment = new Comment
            {
                CommentId = 1,
                Content = "Old comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                TaskId = 10,
                UserId = 20
            };

            var updateDto = new UpdateCommentDto
            {
                Content = "Updated comment"
            };

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(comment);

            _commentRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Comment>()))
                .ThrowsAsync(new Exception("Update failed"));


            // Act

            var act = () => _commentService.UpdateCommentAsync(
                1,
                updateDto
            );


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Update failed");

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<Comment>(c =>
                    c.Content == "Updated comment"
                )),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        [Fact]
        public async Task UpdateCommentAsync_WhenSaveChangesFails_PropagatesException()
        {
            // Arrange

            var comment = new Comment
            {
                CommentId = 1,
                Content = "Old comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                TaskId = 10,
                UserId = 20
            };

            var updateDto = new UpdateCommentDto
            {
                Content = "Updated comment"
            };

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(comment);

            _commentRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Comment>()))
                .Returns(Task.CompletedTask);

            _commentRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Save failed"));


            // Act

            var act = () => _commentService.UpdateCommentAsync(
                1,
                updateDto
            );


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Save failed");

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<Comment>(c =>
                    c.Content == "Updated comment"
                )),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }










        //////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ///last one Delete 
        ///


        // =========================================================
        // DeleteCommentAsync
        // =========================================================

        [Fact]
        public async Task DeleteCommentAsync_WhenCommentExists_SoftDeletesComment()
        {
            // Arrange

            var comment = new Comment
            {
                CommentId = 1,
                Content = "Test comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                TaskId = 10,
                UserId = 20,
                IsDeleted = false
            };

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(comment);

            _commentRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);


            // Act

            await _commentService.DeleteCommentAsync(1);


            // Assert

            comment.IsDeleted.Should().BeTrue();

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }


        [Fact]
        public async Task DeleteCommentAsync_WhenCommentDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Comment?)null);


            // Act

            var act = () => _commentService.DeleteCommentAsync(1);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<NotFoundException>();

            exception.Which.Message.Should()
                .Be("Comment not found.");

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        [Fact]
        public async Task DeleteCommentAsync_WhenGetCommentFails_PropagatesException()
        {
            // Arrange

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));


            // Act

            var act = () => _commentService.DeleteCommentAsync(1);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Database error");

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never
            );
        }


        [Fact]
        public async Task DeleteCommentAsync_WhenSaveChangesFails_PropagatesException()
        {
            // Arrange

            var comment = new Comment
            {
                CommentId = 1,
                Content = "Test comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                TaskId = 10,
                UserId = 20,
                IsDeleted = false
            };

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(comment);

            _commentRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Save failed"));


            // Act

            var act = () => _commentService.DeleteCommentAsync(1);


            // Assert

            var exception = await act.Should()
                .ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Be("Save failed");

            // Comment was found.

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );

            // Save was attempted.

            _commentRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );

            // Even though Save failed, the service did set IsDeleted.

            comment.IsDeleted.Should().BeTrue();
        }


    }

}
