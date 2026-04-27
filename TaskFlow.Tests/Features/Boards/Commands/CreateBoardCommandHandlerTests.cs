using FluentAssertions;
using Moq;
using TaskFlow.Application.Features.Boards.Commands.CreateBoard;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Tests.Features.Boards.Commands;

public class CreateBoardCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateBoardWith3DefaultColumns()
    {
        // ARRANGE
        Board? capturedBoard = null;
        var mockRepo = new Mock<IRepository<Board>>();
        mockRepo
            .Setup(r => r.AddAsync(It.IsAny<Board>()))
            .Callback<Board>(b => capturedBoard = b)
            .ReturnsAsync((Board b) => b);

        var handler = new CreateBoardCommandHandler(mockRepo.Object);
        var command = new CreateBoardCommand("Sprint Board", "Sample description", Guid.NewGuid());

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        capturedBoard.Should().NotBeNull();
        capturedBoard!.Columns.Should().HaveCount(3);
        capturedBoard.Columns.Select(c => c.Name)
            .Should().ContainInOrder("To Do", "In Progress", "Done");
        capturedBoard.Columns.Select(c => c.Position)
            .Should().ContainInOrder(0, 1, 2);

        mockRepo.Verify(r => r.AddAsync(It.IsAny<Board>()), Times.Once);
    }
}
