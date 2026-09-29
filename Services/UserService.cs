using FactorySystem.Data;
using FactorySystem.Endpoints;
using FactorySystem.Models;

namespace FactorySystem.Services;

public class UserService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public UserService(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    
    public UserResponseDto CreateUser(User newUser)
    {
        
        newUser.PasswordHash = _hasher.HashPassword(newUser.PasswordHash);
        
        _db.Users.Add(newUser);
        
        _db.SaveChanges();
        
        var safeUser = new UserResponseDto
        {
            Id = newUser.Id,
            Name = newUser.Name,
            Role = newUser.Role,
        };

        return safeUser;
    }
}