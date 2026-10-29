using FactorySystem.Data;
using FactorySystem.Endpoints;
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
     }
}
