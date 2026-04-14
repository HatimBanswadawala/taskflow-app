using MediatR;

namespace TaskFlow.Application.Features.Tasks.Commands.DeleteTask;

public record DeleteTaskCommand(Guid Id) : IRequest<bool>;
