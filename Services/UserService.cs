using FactorySystem.Data;
using FactorySystem.Endpoints;
using FactorySystem.Models;

namespace FactorySystem.Services;

public class UserService
{
    // Добавляем читалку чтобы наш класс знал что это такое
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    // 
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

    // Удаление юзера
    public bool DeleteUser(int id)
    {
        var deleteUser = _db.Users.Find(id);
        if (deleteUser != null)
        {
            _db.Users.Remove(deleteUser);
            _db.SaveChanges();
            return true;
        }
        else
        {
            return false;
        }
    }
    
    // Получение всех юзеров
    public List<UserResponseDto> GetUsers()
    {
        var allUsers = _db.Users
            .Select(userFromDb => new UserResponseDto
            {
                Id = userFromDb.Id,
                Name = userFromDb.Name,
                Role = userFromDb.Role,
            })
            .ToList();
        return allUsers;
    }
}