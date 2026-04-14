using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponseDto>;
