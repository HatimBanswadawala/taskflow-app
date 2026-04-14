using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Mappers;

/// <summary>
/// Extension methods to map domain entities → DTOs.
/// Avoids duplication across query handlers.
/// Could be replaced with AutoMapper later, but manual mapping is faster
/// and more explicit for a small project.
/// </summary>
public static class BoardMapper
{
    public static BoardDto ToDto(this Board board) => new(
        board.Id,
        board.Name,
        board.Description,
        board.CreatedAt,
        board.User is null
            ? new OwnerDto(Guid.Empty, "", "")
            : new OwnerDto(board.User.Id, board.User.FullName, board.User.Email),
        board.Columns.Select(c => c.ToDto())
    );

    public static ColumnDto ToDto(this Column column) => new(
        column.Id,
        column.Name,
        column.Position,
        column.Tasks.Select(t => t.ToDto())
    );

    public static TaskItemDto ToDto(this TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Priority.ToString(),
        task.Status.ToString(),
        task.DueDate,
        task.Position,
        task.AssignedToId
    );
}
