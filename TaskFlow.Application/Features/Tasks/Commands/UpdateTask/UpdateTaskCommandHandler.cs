using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Commands.UpdateTask;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, bool>
{
    private readonly DbContext _db;

    public UpdateTaskCommandHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _db.Set<TaskItem>().FindAsync([request.Id], cancellationToken);
        if (task is null)
            return false;

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
