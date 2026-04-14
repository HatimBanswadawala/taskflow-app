using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Mappers;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Boards.Queries.GetBoards;

/// <summary>
/// Handler uses DbContext directly (not repository) because Queries need eager loading
/// and complex projections that the generic repository doesn't support.
/// Pattern: Commands use Repository, Queries use DbContext directly.
/// </summary>
public class GetBoardsQueryHandler : IRequestHandler<GetBoardsQuery, IEnumerable<BoardDto>>
{
    private readonly DbContext _db;

    public GetBoardsQueryHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<BoardDto>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
    {
        var boards = await _db.Set<Board>()
            .Include(b => b.User)
            .Include(b => b.Columns.OrderBy(c => c.Position))
                .ThenInclude(c => c.Tasks.OrderBy(t => t.Position))
            .ToListAsync(cancellationToken);

        return boards.Select(b => b.ToDto());
    }
}
