using MediatR;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Tasks.Commands.UpdateTask;

public record UpdateTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    Priority Priority,
    DateTime? DueDate,
    Guid? AssignedToId
) : IRequest<bool>;
