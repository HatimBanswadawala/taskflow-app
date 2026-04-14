using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Boards.Queries.GetBoards;

/// <summary>
/// Query to get all boards with their columns and tasks.
/// Returns IEnumerable&lt;BoardDto&gt; — no entity leaking out of Application layer.
/// </summary>
public record GetBoardsQuery() : IRequest<IEnumerable<BoardDto>>;
