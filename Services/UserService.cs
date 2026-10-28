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
    
    public Result<UserResponseDto> CreateUser(User newUser)
    {
        var userFromDb = _db.Users.FirstOrDefault(u => u.Name == newUser.Name);

        if (userFromDb != null)
        {
            return Result<UserResponseDto>.Failure(new Error(ErrorType.Conflict, "User with that name already exists."));
        }
        
        newUser.PasswordHash = _hasher.HashPassword(newUser.PasswordHash);
        
        _db.Users.Add(newUser);
        
        _db.SaveChanges();
        
        var safeUser = new UserResponseDto
        {
            Id = newUser.Id,
            Name = newUser.Name,
            Role = newUser.Role,
        };

        return Result<UserResponseDto>.Success(safeUser);
    }

    // Удаление юзера
    public Result DeleteUser(int id)
    {
        var deleteUser = _db.Users.Find(id);
        if (deleteUser != null)
        {
            _db.Users.Remove(deleteUser);
            _db.SaveChanges();
            return Result.Success();
        }
        else
        {
            return Result.Failure( new Error ( ErrorType.NotFound, "User not found on the factory floor." ) );
        }
    }
    
    // Получение всех юзеров
    public Result<List<UserResponseDto>> GetUsersAll()
    {
        var allUsers = _db.Users
            .Select(userFromDb => new UserResponseDto
            {
                Id = userFromDb.Id,
                Name = userFromDb.Name,
                Role = userFromDb.Role,
            })
            .ToList();
        return Result<List<UserResponseDto>>.Success(allUsers);
    }
    
    // Получение конкретного юзера
    public Result<UserResponseDto> GetUser(int id)
    {
        var user = _db.Users.Find(id);

        if (user != null)
        {
            
            var safeUser = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Role = user.Role
            };

            return Result<UserResponseDto>.Success(safeUser);
        }
        else
        {
            return Result<UserResponseDto>.Failure(new Error(ErrorType.NotFound, "User not found on the factory floor."));
        }
    }
    
    
    
    // Обновляем юзера
    public Result<UserResponseDto> UpdateUser(int id, UpdateUserDto dto)
    {

        var user = _db.Users.Find(id);

        if (user != null)
        {
            user.Name = dto.Name;
            user.Role = dto.Role;
            _db.SaveChanges();
            
            var safeUser = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Role = user.Role
            };

            return Result<UserResponseDto>.Success(safeUser);
        }
        else
        {
            return Result<UserResponseDto>.Failure(new Error(ErrorType.NotFound, "User not found on the factory floor." ) );
        }
    }
}