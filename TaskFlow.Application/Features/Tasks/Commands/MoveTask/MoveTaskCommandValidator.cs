using FluentValidation;

namespace TaskFlow.Application.Features.Tasks.Commands.MoveTask;

public class MoveTaskCommandValidator : AbstractValidator<MoveTaskCommand>
{
    public MoveTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.TargetColumnId)
            .NotEmpty().WithMessage("Target column ID is required");

        RuleFor(x => x.NewPosition)
            .GreaterThanOrEqualTo(0).WithMessage("Position must be 0 or greater");
    }
}
