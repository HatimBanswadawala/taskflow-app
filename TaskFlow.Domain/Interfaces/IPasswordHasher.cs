namespace TaskFlow.Domain.Interfaces;

/// <summary>
/// Domain says "I need something that can hash and verify passwords."
/// Infrastructure provides the BCrypt implementation.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hashedPassword);
}
