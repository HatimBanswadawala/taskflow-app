using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Features.Tasks.Commands.MoveTask;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Tests.Features.Tasks.Commands;

public class MoveTaskCommandHandlerTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    /// <summary>
    /// Seeds a board with 2 columns (To Do, Done) and a single task in the To Do column.
    /// Returns IDs the test will use to construct the move command.
    /// </summary>
    private static (Guid taskId, Guid todoColumnId, Guid doneColumnId)
        SeedBoardWithTask(AppDbContext db)
    {
        var userId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var todoColumnId = Guid.NewGuid();
        var doneColumnId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        db.Users.Add(new User { Id = userId, FullName = "U", Email = "u@e.com", PasswordHash = "h" });
        db.Boards.Add(new Board { Id = boardId, Name = "B", UserId = userId });
        db.Columns.Add(new Column { Id = todoColumnId, Name = "To Do", Position = 0, BoardId = boardId });
        db.Columns.Add(new Column { Id = doneColumnId, Name = "Done", Position = 1, BoardId = boardId });
        db.TaskItems.Add(new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            ColumnId = todoColumnId,
            Position = 0,
            Status = TaskItemStatus.Todo
        });
        db.SaveChanges();
        return (taskId, todoColumnId, doneColumnId);
    }

    [Fact]
    public async Task Handle_ShouldUpdateColumnAndStatus_WhenMovingTaskToDoneColumn()
    {
        // ARRANGE
        await using var db = CreateInMemoryDb();
        var (taskId, _, doneColumnId) = SeedBoardWithTask(db);

        var handler = new MoveTaskCommandHandler(db);
        var command = new MoveTaskCommand(taskId, doneColumnId, NewPosition: 0);

        // ACT
        var result = await handler.Handle(command, CancellationToken.None);

        // ASSERT — operation succeeded
        result.Should().BeTrue();

        // ASSERT — task moved to "Done" column with correct status
        var movedTask = await db.TaskItems.FindAsync(taskId);
        movedTask.Should().NotBeNull();
        movedTask!.ColumnId.Should().Be(doneColumnId);
        movedTask.Position.Should().Be(0);
        movedTask.Status.Should().Be(TaskItemStatus.Done);   // auto-status update
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        // ARRANGE
        await using var db = CreateInMemoryDb();
        var (_, _, doneColumnId) = SeedBoardWithTask(db);

        var handler = new MoveTaskCommandHandler(db);
        var command = new MoveTaskCommand(
            TaskId: Guid.NewGuid(),       // task that doesn't exist
            TargetColumnId: doneColumnId,
            NewPosition: 0);

        // ACT
        var result = await handler.Handle(command, CancellationToken.None);

        // ASSERT — graceful failure, no exception
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenTargetColumnDoesNotExist()
    {
        // ARRANGE
        await using var db = CreateInMemoryDb();
        var (taskId, _, _) = SeedBoardWithTask(db);

        var handler = new MoveTaskCommandHandler(db);
        var command = new MoveTaskCommand(
            TaskId: taskId,
            TargetColumnId: Guid.NewGuid(),  // column that doesn't exist
            NewPosition: 0);

        // ACT
        var result = await handler.Handle(command, CancellationToken.None);

        // ASSERT — graceful failure, original task unchanged
        result.Should().BeFalse();

        var unchangedTask = await db.TaskItems.FindAsync(taskId);
        unchangedTask!.Status.Should().Be(TaskItemStatus.Todo);  // status didn't change
    }
}
