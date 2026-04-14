using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly DbContext _db;

    public DeleteTaskCommandHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _db.Set<TaskItem>().FindAsync([request.Id], cancellationToken);
        if (task is null)
            return false;

        _db.Set<TaskItem>().Remove(task);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
