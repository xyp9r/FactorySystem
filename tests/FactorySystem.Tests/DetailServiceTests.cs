using FactorySystem.Data;
using FactorySystem.Models;
using FactorySystem.Services;
using Microsoft.EntityFrameworkCore;
using FactorySystem.Endpoints;
using FluentValidation;
using FluentValidation.Results;
using System.Security.Claims;
using Moq;

namespace FactorySystem.Tests;

public class DetailServiceTests
{
    
    // Добавляю читалку для общей группы

    private readonly AppDbContext _fakeDb;
    private readonly Mock<IValidator<CreateDetailDto>> _fakeValidator;
    private readonly DetailService _service;

    public DetailServiceTests()
    {
        // Генерируем уникальное имя для бд
        var dbName = Guid.NewGuid().ToString();
        
        // Собираем настройки бд
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        
        // Создаем объекты и записываем их в приватные поля
        _fakeDb = new AppDbContext(options);
        
        _fakeValidator = new Mock<IValidator<CreateDetailDto>>();
        _fakeValidator.Setup(v => v.Validate(It.IsAny<CreateDetailDto>())).Returns(new ValidationResult());
        
        _service = new DetailService(_fakeDb, _fakeValidator.Object);
    }

    [Fact]
    public void Test1_CreateDetail_ReturnSuccess()
    {

        List<Claim> claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.Name, "John Doe"));

        var identity = new ClaimsIdentity(claims, "Test");

        var principal = new ClaimsPrincipal(identity);

        var testUser = new User
        {
            Name = "John Doe",
            Id = 777,
            Role = "Worker",
            PasswordHash = "123"
        };
        
        _fakeDb.Users.Add(testUser);
        _fakeDb.SaveChanges();
        
        var safeDetail = new CreateDetailDto
        {
            Name = "TestDetail",
            Count = 6,
            Status = "В процессе",
        };
        
        var result = _service.CreateDetail(safeDetail, principal);
        
        Assert.True(result.IsSuccess);

        var createdDetail = _fakeDb.Details.Find(result.Value.Id);
        
        Assert.NotNull(createdDetail);
        Assert.Equal(testUser.Id, createdDetail.CreatorId);
        Assert.Equal(safeDetail.Name, createdDetail.Name);
        Assert.Equal(safeDetail.Count, createdDetail.Count);
        Assert.Equal(safeDetail.Status, createdDetail.Status);
    }
    
}