using MediatR;

namespace TaskFlow.Application.Features.Boards.Commands.CreateBoard;

/// <summary>
/// Command to create a new board.
/// IRequest<Guid> means this command returns a Guid (the new board's Id) when handled.
/// </summary>
public record CreateBoardCommand(
    string Name,
    string? Description,
    Guid UserId
) : IRequest<Guid>;
