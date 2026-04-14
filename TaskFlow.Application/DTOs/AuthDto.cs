namespace TaskFlow.Application.DTOs;

/// <summary>
/// Response returned after successful login — token + basic user info.
/// Frontend stores the token and sends it with every API request.
/// </summary>
public record AuthResponseDto(
    string Token,
    Guid UserId,
    string FullName,
    string Email);
