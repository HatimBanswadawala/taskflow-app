using MediatR;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Tasks.Commands.CreateTask;

public record CreateTaskCommand(
    string Title,
    string? Description,
    Priority Priority,
    Guid ColumnId,
    Guid? AssignedToId,
    DateTime? DueDate
) : IRequest<Guid>;
