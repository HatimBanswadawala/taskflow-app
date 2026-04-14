using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly DbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        DbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        // Password matches — generate JWT token
        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto(token, user.Id, user.FullName, user.Email);
    }
}
