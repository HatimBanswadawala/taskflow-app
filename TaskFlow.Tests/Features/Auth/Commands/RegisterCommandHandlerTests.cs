using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskFlow.Application.Features.Auth.Commands.Register;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Tests.Features.Auth.Commands;

public class RegisterCommandHandlerTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())  // unique per test
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_BeforeStoringUser()
    {
        // ARRANGE
        await using var db = CreateInMemoryDb();

        var mockHasher = new Mock<IPasswordHasher>();
        mockHasher.Setup(h => h.Hash("plaintextPassword"))
                  .Returns("HASHED_VALUE_123");

        var mockJwt = new Mock<IJwtTokenService>();
        mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>()))
               .Returns("dummy.jwt.token");

        var handler = new RegisterCommandHandler(db, mockHasher.Object, mockJwt.Object);
        var command = new RegisterCommand("Hatim", "test@example.com", "plaintextPassword");

        // ACT
        var response = await handler.Handle(command, CancellationToken.None);

        // ASSERT — user was stored with HASHED password (NOT plaintext)
        var storedUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        storedUser.Should().NotBeNull();
        storedUser!.PasswordHash.Should().Be("HASHED_VALUE_123");
        storedUser.PasswordHash.Should().NotBe("plaintextPassword");

        // VERIFY — hasher was called exactly once with the plaintext password
        mockHasher.Verify(h => h.Hash("plaintextPassword"), Times.Once);

        // VERIFY — JWT token returned in response
        response.Token.Should().Be("dummy.jwt.token");
        response.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailAlreadyExists()
    {
        // ARRANGE
        await using var db = CreateInMemoryDb();

        // Pre-seed an existing user
        db.Users.Add(new User
        {
            FullName = "Existing User",
            Email = "duplicate@example.com",
            PasswordHash = "old_hash"
        });
        await db.SaveChangesAsync();

        var mockHasher = new Mock<IPasswordHasher>();
        var mockJwt = new Mock<IJwtTokenService>();

        var handler = new RegisterCommandHandler(db, mockHasher.Object, mockJwt.Object);
        var command = new RegisterCommand("New User", "duplicate@example.com", "password");

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("A user with this email already exists");

        // VERIFY — hasher NEVER called when email duplicate (no user created)
        mockHasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }
}
