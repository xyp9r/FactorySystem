namespace FactorySystem.Endpoints;
using BCrypt.Net;
using BCryptInstance = BCrypt.Net.BCrypt;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string password, string hashedPassword);
}

public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;
    
    public string HashPassword(string password) => 
        BCryptInstance.HashPassword(password, WorkFactor);
    
    public bool VerifyHashedPassword(string password, string hashedPassword) =>
        BCryptInstance.Verify(password, hashedPassword);
}