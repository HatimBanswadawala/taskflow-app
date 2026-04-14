using MediatR;

namespace TaskFlow.Application.Features.Tasks.Commands.MoveTask;

/// <summary>
/// Moves a task to a different column and/or changes its position within the column.
/// This is the backend for drag-and-drop.
/// </summary>
public record MoveTaskCommand(
    Guid TaskId,
    Guid TargetColumnId,
    int NewPosition
) : IRequest<bool>;
