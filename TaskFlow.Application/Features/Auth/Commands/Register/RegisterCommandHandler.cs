using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly DbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        DbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _db.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser is not null)
            throw new InvalidOperationException("A user with this email already exists");

        // Create user with hashed password
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        _db.Set<User>().Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        // Generate JWT token and return — user is logged in immediately after registration
        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto(token, user.Id, user.FullName, user.Email);
    }
}
