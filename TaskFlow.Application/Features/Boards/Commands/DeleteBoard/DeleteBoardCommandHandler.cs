using MediatR;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Features.Boards.Commands.DeleteBoard;

public class DeleteBoardCommandHandler : IRequestHandler<DeleteBoardCommand, bool>
{
    private readonly IRepository<Board> _boardRepository;

    public DeleteBoardCommandHandler(IRepository<Board> boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<bool> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(request.Id);
        if (board is null)
            return false;

        await _boardRepository.DeleteAsync(request.Id);
        return true;
    }
}
