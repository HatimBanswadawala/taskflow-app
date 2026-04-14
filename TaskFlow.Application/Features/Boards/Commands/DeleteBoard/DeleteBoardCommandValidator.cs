using FluentValidation;

namespace TaskFlow.Application.Features.Boards.Commands.DeleteBoard;

public class DeleteBoardCommandValidator : AbstractValidator<DeleteBoardCommand>
{
    public DeleteBoardCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Board ID is required");
    }
}
