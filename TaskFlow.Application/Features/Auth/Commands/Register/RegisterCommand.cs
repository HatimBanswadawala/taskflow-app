using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string FullName,
    string Email,
    string Password
) : IRequest<AuthResponseDto>;
