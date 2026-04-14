using FluentValidation;

namespace TaskFlow.Application.Features.Boards.Commands.UpdateBoard;

public class UpdateBoardCommandValidator : AbstractValidator<UpdateBoardCommand>
{
    public UpdateBoardCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Board ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Board name is required")
            .MaximumLength(100).WithMessage("Board name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}
