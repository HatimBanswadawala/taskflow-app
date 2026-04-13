using MediatR;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Features.Boards.Commands.CreateBoard;

/// <summary>
/// Handles the CreateBoardCommand — this is where the actual logic lives.
/// MediatR auto-wires this to the Command via the IRequestHandler interface.
/// </summary>
public class CreateBoardCommandHandler : IRequestHandler<CreateBoardCommand, Guid>
{
    private readonly IRepository<Board> _boardRepository;

    public CreateBoardCommandHandler(IRepository<Board> boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Guid> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = new Board
        {
            Name = request.Name,
            Description = request.Description,
            UserId = request.UserId
        };

        // When a board is created, automatically add 3 default columns
        board.Columns = new List<Column>
        {
            new() { Name = "To Do", Position = 0, BoardId = board.Id },
            new() { Name = "In Progress", Position = 1, BoardId = board.Id },
            new() { Name = "Done", Position = 2, BoardId = board.Id }
        };

        var created = await _boardRepository.AddAsync(board);
        return created.Id;
    }
}
