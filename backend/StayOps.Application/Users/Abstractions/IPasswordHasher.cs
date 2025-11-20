namespace StayOps.Application.Users.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string password);
}
