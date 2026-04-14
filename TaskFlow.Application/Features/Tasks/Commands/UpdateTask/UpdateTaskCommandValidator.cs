using FluentValidation;

namespace TaskFlow.Application.Features.Tasks.Commands.UpdateTask;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");
    }
}
