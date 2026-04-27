using FluentValidation.TestHelper;
using TaskFlow.Application.Features.Boards.Commands.CreateBoard;

namespace TaskFlow.Tests.Features.Boards.Commands;

public class CreateBoardCommandValidatorTests
{
    private readonly CreateBoardCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenNameIsEmptyOrNull(string? badName)
    {
        // ARRANGE
        var command = new CreateBoardCommand(badName!, "desc", Guid.NewGuid());

        // ACT
        var result = _validator.TestValidate(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(c => c.Name)
              .WithErrorMessage("Board name is required");
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameExceeds100Characters()
    {
        // ARRANGE
        var longName = new string('A', 101);  // 101 'A' chars
        var command = new CreateBoardCommand(longName, "desc", Guid.NewGuid());

        // ACT
        var result = _validator.TestValidate(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(c => c.Name)
              .WithErrorMessage("Board name cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsValid()
    {
        // ARRANGE
        var command = new CreateBoardCommand("Sprint 1", "Description", Guid.NewGuid());

        // ACT
        var result = _validator.TestValidate(command);

        // ASSERT
        result.ShouldNotHaveAnyValidationErrors();
    }
}
