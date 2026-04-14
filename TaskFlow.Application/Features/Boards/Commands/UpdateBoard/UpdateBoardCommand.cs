using MediatR;

namespace TaskFlow.Application.Features.Boards.Commands.UpdateBoard;

/// <summary>
/// Command to update an existing board's name and description.
/// Returns bool — true if updated, false if not found.
/// </summary>
public record UpdateBoardCommand(
    Guid Id,
    string Name,
    string? Description
) : IRequest<bool>;
