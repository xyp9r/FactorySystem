using FactorySystem.Data;
using FactorySystem.Endpoints;
using FactorySystem.Models;
using FactorySystem.Services;
using Microsoft.EntityFrameworkCore;

namespace FactorySystem.Tests;

using Moq;

public class UnitTest1
{
    [Fact]
    public void Test1()
     {
         
         // Arrange - Подготовка: собираем зависимости, фейки и входные данные
        var hasherMock = new Mock<IPasswordHasher>();

        hasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("fake_hash");
        
        var fakeHasher = hasherMock.Object;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestFactorydb")
            .Options;

        var fakeDb = new AppDbContext(options);

        var service = new UserService(fakeDb, fakeHasher);

        var testUser = new User
        {
            Name = "TestUser",
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };

        // Act - Действие: вызываем ровно ОДИН метод, который хотим протестировать
        var result = service.CreateUser(testUser);

        
        // Assert - Проверка: убеждаемся, что метод отработал так как мы ожидали
        Assert.True(result.IsSuccess);
        Assert.Equal("TestUser", result.Value.Name);
     }
    
    [Fact]
    public void Test2_UserAlreadyExists()
    {
     
        // Arrange - Подготовка: собираем зависимости, фейки и входные данные
        var hasherMock = new Mock<IPasswordHasher>();

        hasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("fake_hash");
        
        var fakeHasher = hasherMock.Object;
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestFactorydb")
            .Options;
        
        var fakeDb = new AppDbContext(options);
        
        var service = new UserService(fakeDb, fakeHasher);

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
        
        fakeDb.Users.Add(existingUser);
        fakeDb.SaveChanges();

        // Act - Действие: вызываем ровно ОДИН метод, который хотим протестировать
        var result = service.CreateUser(testUser);
        
        // Assert - Проверка: убеждаемся, что метод отработал так как мы ожидали
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void Test3_EmptyName_ReturnsError()
    {
        // Arrange - Собираем зависимости
        var hasherMock = new Mock<IPasswordHasher>();
        
        hasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("fake_hash");

        var fakeHasher = hasherMock.Object;
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestFactorydb")
            .Options;

        var fakeDb = new AppDbContext(options);

        var service = new UserService(fakeDb, fakeHasher);

        var testUser = new User
        {
            Name = "",
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };

        // Act - действие: создаем нашего подопытного юзера
        var result = service.CreateUser(testUser);
        
        // Assert - Проверка, убеждаемся что метод отработал как мы и ожидали
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validator, result.Error.Type);
    }

    [Fact]
    public void Test4_EmptyPassword_ReturnsError()
    {
        // Arrange - собираем зависимости
        var hasherMock = new Mock<IPasswordHasher>();
        
        hasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("fake_hash");

        var fakeHasher = hasherMock.Object;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestFactorydb")
            .Options;

        var fakeDb = new AppDbContext(options);

        var service = new UserService(fakeDb, fakeHasher);

        var testUser = new User
        {
            Name = "TestUser",
            PasswordHash = "",
            Role = "TestRole"
        };
        
        // Act - Создаем юзера
        var result = service.CreateUser(testUser);

        // Assert - Проверка, убеждаемся что метод отработал как мы и ожидали
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validator, result.Error.Type);
    }

    [Fact]
    public void Test5_DeleteUser_ReturnsSuccess()
    {
        var hasherMock = new Mock<IPasswordHasher>();
        
        hasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("fake_hash");

        var fakeHasher = hasherMock.Object;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestFactorydb")
            .Options;

        var fakeDb = new AppDbContext(options);

        var service = new UserService(fakeDb, fakeHasher);

        var testUser = new User
        {
            Name = "TestUser",
            Id = 777,
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };

        fakeDb.Users.Add(testUser);
        fakeDb.SaveChanges();

        var delete = service.DeleteUser(testUser.Id);
        
        var deletedUser = fakeDb.Users.Find(testUser.Id);
        
        Assert.True(delete.IsSuccess);
        Assert.Null(deletedUser);
    }

    [Fact]
    public void Test6_DeleteNonExistentUser_ReturnsError()
    {
        var hasherMock = new Mock<IPasswordHasher>();
        
        hasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("fake_hash");

        var fakeHasher = hasherMock.Object;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestFactorydb")
            .Options;

        var fakeDb = new AppDbContext(options);

        var service = new UserService(fakeDb, fakeHasher);

        var result = service.DeleteUser(777);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public void Test7_GetUsersAll_ReturnsSuccess()
    {
        var hasherMock = new Mock<IPasswordHasher>();
        
        hasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("fake_hash");

        var fakeHasher = hasherMock.Object;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestFactorydb")
            .Options;

        var fakeDb = new AppDbContext(options);
        
        var service = new UserService(fakeDb, fakeHasher);

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

        fakeDb.Users.Add(testUser);
        fakeDb.Users.Add(testUser2);
        fakeDb.SaveChanges();

        var get = service.GetUsersAll();
        
        Assert.True(get.IsSuccess);
        Assert.Equal(2, get.Value.Count);
    }
}
