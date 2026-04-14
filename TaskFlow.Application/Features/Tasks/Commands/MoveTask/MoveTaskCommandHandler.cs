using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Tasks.Commands.MoveTask;

/// <summary>
/// Handles moving a task between columns (drag-and-drop).
/// Updates the task's column, position, and status.
/// Also reorders other tasks in both source and target columns.
/// </summary>
public class MoveTaskCommandHandler : IRequestHandler<MoveTaskCommand, bool>
{
    private readonly DbContext _db;

    public MoveTaskCommandHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _db.Set<TaskItem>().FindAsync([request.TaskId], cancellationToken);
        if (task is null)
            return false;

        var targetColumn = await _db.Set<Column>().FindAsync([request.TargetColumnId], cancellationToken);
        if (targetColumn is null)
            return false;

        var sourceColumnId = task.ColumnId;
        var isMovingColumns = sourceColumnId != request.TargetColumnId;

        if (isMovingColumns)
        {
            // Reorder source column — close the gap where the task left
            var sourceTasks = await _db.Set<TaskItem>()
                .Where(t => t.ColumnId == sourceColumnId && t.Id != task.Id)
                .OrderBy(t => t.Position)
                .ToListAsync(cancellationToken);

            for (int i = 0; i < sourceTasks.Count; i++)
                sourceTasks[i].Position = i;
        }

        // Reorder target column — make room at the new position
        var targetTasks = await _db.Set<TaskItem>()
            .Where(t => t.ColumnId == request.TargetColumnId && t.Id != task.Id)
            .OrderBy(t => t.Position)
            .ToListAsync(cancellationToken);

        // Insert at new position — shift everything at or after down by 1
        for (int i = 0; i < targetTasks.Count; i++)
        {
            targetTasks[i].Position = i >= request.NewPosition ? i + 1 : i;
        }

        // Move the task
        task.ColumnId = request.TargetColumnId;
        task.Position = request.NewPosition;
        task.UpdatedAt = DateTime.UtcNow;

        // Auto-update status based on column name (smart UX)
        task.Status = targetColumn.Name.ToLower() switch
        {
            "to do" => TaskItemStatus.Todo,
            "in progress" => TaskItemStatus.InProgress,
            "done" => TaskItemStatus.Done,
            _ => task.Status // Keep current status for custom columns
        };

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
