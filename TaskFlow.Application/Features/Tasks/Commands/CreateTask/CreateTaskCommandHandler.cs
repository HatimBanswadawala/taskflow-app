using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Guid>
{
    private readonly DbContext _db;

    public CreateTaskCommandHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // Verify the column exists
        var column = await _db.Set<Column>().FindAsync([request.ColumnId], cancellationToken);
        if (column is null)
            throw new InvalidOperationException($"Column {request.ColumnId} not found");

        // Calculate position — new task goes to the bottom of the column
        var maxPosition = await _db.Set<TaskItem>()
            .Where(t => t.ColumnId == request.ColumnId)
            .MaxAsync(t => (int?)t.Position, cancellationToken) ?? -1;

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = TaskItemStatus.Todo,
            ColumnId = request.ColumnId,
            AssignedToId = request.AssignedToId,
            DueDate = request.DueDate,
            Position = maxPosition + 1 // Append to bottom
        };

        _db.Set<TaskItem>().Add(task);
        await _db.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
