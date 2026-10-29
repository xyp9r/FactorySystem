using FactorySystem.Data;
using FactorySystem.Endpoints;
using FactorySystem.Models;
using FactorySystem.Services;
using Microsoft.EntityFrameworkCore;

namespace FactorySystem.FactorySystem.Tests;

using Moq;

public class UnitTest1
{
    [Fact]
    public void Test1()
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
            PasswordHash = "fake_hash",
            Role = "TestRole"
        };

        var result = service.CreateUser(testUser);

        Assert.True(result.IsSuccess);
        
        Assert.Equal("TestUser", result.Value.Name);
     }
}
