using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Interfaces;

/// <summary>
/// Domain says "I need something that can generate JWT tokens for a user."
/// Infrastructure provides the implementation with signing keys and claims.
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(User user);
}
