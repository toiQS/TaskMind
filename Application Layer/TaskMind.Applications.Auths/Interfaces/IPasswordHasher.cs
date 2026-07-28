// IPasswordHasher.cs
namespace TaskMind.Applications.Auths.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string passwordHash);
    }
}