using FactorySystem.Data;
using FactorySystem.Endpoints;
using FactorySystem.Models;
using FactorySystem.Services;
using Microsoft.EntityFrameworkCore;

namespace FactorySystem.Tests;

using Moq;

public class UserTests
{
    
    // Добавляю читалку для общей штуки чтобы её не копипастить

    private readonly AppDbContext _fakeDb;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly UserService _service;

    public UserTests()
    {
        // Генерируем уникальное имя для бд
        var dbName = Guid.NewGuid().ToString();
        
        // Собираем настройки бд
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        
        // Создаем объекты и записываем их в приватные поля
        _fakeDb = new AppDbContext(options);

        _hasherMock = new Mock<IPasswordHasher>();
        _hasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("fake_hash");
        
        _service = new UserService(_fakeDb, _hasherMock.Object);
    }
    
    [Fact]
    public void Test1_CreateUser_ReturnsSuccess()
     {
         
        var testUser = new User
        {
            Name = "TestUser",
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };

        // Act - Действие: вызываем ровно ОДИН метод, который хотим протестировать
        var result = _service.CreateUser(testUser);

        
        // Assert - Проверка: убеждаемся, что метод отработал так как мы ожидали
        Assert.True(result.IsSuccess);
        Assert.Equal("TestUser", result.Value.Name);
        
     }
    
    [Fact]
    public void Test2_UserAlreadyExists_ReturnsError()
    {

        var existingUser = new User
        {
            Name = "Ivan",
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };

        var testUser = new User
        {
            Name = "Ivan",
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };
        
        _fakeDb.Users.Add(existingUser);
        _fakeDb.SaveChanges();

        // Act - Действие: вызываем ровно ОДИН метод, который хотим протестировать
        var result = _service.CreateUser(testUser);
        
        // Assert - Проверка: убеждаемся, что метод отработал так как мы ожидали
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void Test3_EmptyName_ReturnsError()
    {

        var testUser = new User
        {
            Name = "",
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };

        // Act - действие: создаем нашего подопытного юзера
        var result = _service.CreateUser(testUser);
        
        // Assert - Проверка, убеждаемся что метод отработал как мы и ожидали
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validator, result.Error.Type);
    }

    [Fact]
    public void Test4_EmptyPassword_ReturnsError()
    {
        
        var testUser = new User
        {
            Name = "TestUser",
            PasswordHash = "",
            Role = "TestRole"
        };
        
        // Act - Создаем юзера
        var result = _service.CreateUser(testUser);

        // Assert - Проверка, убеждаемся что метод отработал как мы и ожидали
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validator, result.Error.Type);
    }

    [Fact]
    public void Test5_DeleteUser_ReturnsSuccess()
    {
        
        var testUser = new User
        {
            Name = "TestUser",
            Id = 777,
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };

        _fakeDb.Users.Add(testUser);
        _fakeDb.SaveChanges();

        var delete = _service.DeleteUser(testUser.Id);
        
        var deletedUser = _fakeDb.Users.Find(testUser.Id);
        
        Assert.True(delete.IsSuccess);
        Assert.Null(deletedUser);
    }

    [Fact]
    public void Test6_DeleteNonExistentUser_ReturnsError()
    {

        var result = _service.DeleteUser(777);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public void Test7_GetUsersAll_ReturnsSuccess()
    {
        
        var testUser = new User
        {
            Name = "TestUser",
            Id = 1,
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };
        
        var testUser2 = new User
        {
            Name = "TestUser2",
            Id = 2,
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };

        _fakeDb.Users.Add(testUser);
        _fakeDb.Users.Add(testUser2);
        _fakeDb.SaveChanges();

        var get = _service.GetUsersAll();
        
        Assert.True(get.IsSuccess);
        Assert.Equal(2, get.Value.Count);
    }
    
    [Fact]
    public void Test8_GetUserById_ReturnsSuccess()
    {
        
        var testUser = new User
        {
            Name = "TestUser",
            Id = 1337,
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };

        _fakeDb.Users.Add(testUser);
        _fakeDb.SaveChanges();

        var result = _service.GetUser(1337);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(1337, result.Value.Id);
    }
    
    [Fact]
    public void Test9_GetUserById_ReturnsError()
    {
        
        var result = _service.GetUser(1337);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public void Test10_UpdateUser_ReturnsSuccess()
    {
        
        var oldUser = new User
        {
            Name = "OldUser",
            Id = 777,
            PasswordHash = "fake_hash",
            Role = "OldRole"
        };

        _fakeDb.Users.Add(oldUser);
        _fakeDb.SaveChanges();

        var newUser = new UpdateUserDto
        {
            Name = "NewUser"
        };

        var result = _service.UpdateUser(777, newUser);

        var updatedUser = _fakeDb.Users.Find(777);
        
        Assert.True(result.IsSuccess);
        Assert.Equal("NewUser", updatedUser.Name);
    }

    [Fact]
    public void Test11_UpdateNonExistentUser_ReturnsError()
    {
        
        var result = _service.UpdateUser(777, new UpdateUserDto());
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
