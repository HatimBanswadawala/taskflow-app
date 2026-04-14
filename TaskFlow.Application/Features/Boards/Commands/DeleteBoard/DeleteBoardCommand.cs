using MediatR;

namespace TaskFlow.Application.Features.Boards.Commands.DeleteBoard;

public record DeleteBoardCommand(Guid Id) : IRequest<bool>;
