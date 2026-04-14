using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// BCrypt password hashing — industry standard.
/// BCrypt is "slow by design" which makes brute-force attacks impractical.
/// The hash includes a salt automatically — no need to store salt separately.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        // workFactor 12 = ~250ms per hash. High enough to slow attackers,
        // low enough to not annoy users during login.
    }

    public bool Verify(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
