using MediatR;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Features.Boards.Commands.UpdateBoard;

public class UpdateBoardCommandHandler : IRequestHandler<UpdateBoardCommand, bool>
{
    private readonly IRepository<Board> _boardRepository;

    public UpdateBoardCommandHandler(IRepository<Board> boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<bool> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(request.Id);
        if (board is null)
            return false;

        board.Name = request.Name;
        board.Description = request.Description;

        await _boardRepository.UpdateAsync(board);
        return true;
    }
}
