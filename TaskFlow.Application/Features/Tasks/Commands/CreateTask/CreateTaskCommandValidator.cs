using FluentValidation;

namespace TaskFlow.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.ColumnId)
            .NotEmpty().WithMessage("Column ID is required — task must belong to a column");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue); // Only validate if provided
    }
}
