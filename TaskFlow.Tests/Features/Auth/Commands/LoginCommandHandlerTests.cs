using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskFlow.Application.Features.Auth.Commands.Login;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Tests.Features.Auth.Commands;

public class LoginCommandHandlerTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static User SeedUser(AppDbContext db, string email = "user@example.com",
                                 string passwordHash = "hashed_value")
    {
        var user = new User
        {
            FullName = "Test User",
            Email = email,
            PasswordHash = passwordHash
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsValid()
    {
        // ARRANGE
        await using var db = CreateInMemoryDb();
        var seededUser = SeedUser(db, "valid@example.com", "HASHED_VALUE");

        var mockHasher = new Mock<IPasswordHasher>();
        mockHasher.Setup(h => h.Verify("correctPassword", "HASHED_VALUE"))
                  .Returns(true);

        var mockJwt = new Mock<IJwtTokenService>();
        mockJwt.Setup(j => j.GenerateToken(It.Is<User>(u => u.Email == "valid@example.com")))
               .Returns("valid.jwt.token");

        var handler = new LoginCommandHandler(db, mockHasher.Object, mockJwt.Object);
        var command = new LoginCommand("valid@example.com", "correctPassword");

        // ACT
        var response = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        response.Token.Should().Be("valid.jwt.token");
        response.Email.Should().Be("valid@example.com");
        response.UserId.Should().Be(seededUser.Id);

        // VERIFY hasher was called with exact arguments (plaintext + stored hash)
        mockHasher.Verify(h => h.Verify("correctPassword", "HASHED_VALUE"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPasswordIsWrong()
    {
        // ARRANGE
        await using var db = CreateInMemoryDb();
        SeedUser(db, "user@example.com", "HASHED_VALUE");

        var mockHasher = new Mock<IPasswordHasher>();
        mockHasher.Setup(h => h.Verify("wrongPassword", "HASHED_VALUE"))
                  .Returns(false);  // password doesn't match

        var mockJwt = new Mock<IJwtTokenService>();

        var handler = new LoginCommandHandler(db, mockHasher.Object, mockJwt.Object);
        var command = new LoginCommand("user@example.com", "wrongPassword");

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT — generic message (security: prevents email enumeration)
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("Invalid email or password");

        // VERIFY — JWT was NEVER generated when password wrong
        mockJwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailNotFound()
    {
        // ARRANGE
        await using var db = CreateInMemoryDb();  // empty DB — no users

        var mockHasher = new Mock<IPasswordHasher>();
        var mockJwt = new Mock<IJwtTokenService>();

        var handler = new LoginCommandHandler(db, mockHasher.Object, mockJwt.Object);
        var command = new LoginCommand("nobody@example.com", "anyPassword");

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT — same message as wrong-password (security best practice)
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("Invalid email or password");

        // VERIFY — neither hasher nor JWT called when email doesn't exist
        mockHasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        mockJwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }
}
