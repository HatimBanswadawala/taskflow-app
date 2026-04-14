using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Mappers;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Boards.Queries.GetBoardById;

public class GetBoardByIdQueryHandler : IRequestHandler<GetBoardByIdQuery, BoardDto?>
{
    private readonly DbContext _db;

    public GetBoardByIdQueryHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<BoardDto?> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        var board = await _db.Set<Board>()
            .Include(b => b.User)
            .Include(b => b.Columns.OrderBy(c => c.Position))
                .ThenInclude(c => c.Tasks.OrderBy(t => t.Position))
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        return board?.ToDto();
    }
}
