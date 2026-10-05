using System.Security.Claims;
using FactorySystem.Data;
using FactorySystem.Endpoints;
using FactorySystem.Models;
using FluentValidation;


namespace FactorySystem.Services;

public class DetailService
{
    // Добавляем читалку чтобы наш класс знал вообще что это такое
    private readonly AppDbContext _db;
    private readonly IValidator<CreateDetailDto> _validator;

    public DetailService(AppDbContext db, IValidator<CreateDetailDto> validator)
    {
        _db = db;
        _validator = validator;
    }

    // Создаем деталь
    public DetailResponseDto? CreateDetail(CreateDetailDto dto, ClaimsPrincipal principal)
    {
        // Добавляем проверку до того как лезим в бд 
        var validationResult = _validator.Validate(dto);

        // если результат НЕ ВАЛИДНЫЙ выдаём ошибку
        if (!validationResult.IsValid)
        {
            return null;
        }
    
        // читаем имя из токена
        var currentUserName = principal.FindFirstValue(ClaimTypes.Name);
    
        // Делаем запрос к бозе - ищем пользователя у которого Name = currentUserName
        var userFromDb = _db.Users.FirstOrDefault(u => u.Name == currentUserName);
    
        // Защита от дурака: а вдруг юзера уже удалили из базы, а токен у него ещё жив
        if (userFromDb == null)
        {
            return null;
        }

        var realDetail = new Detail
        {
            Name = dto.Name,
            Count = dto.Count,
            Status = dto.Status,
            CreatorId = userFromDb.Id
        };
        
        _db.Details.Add(realDetail);
        _db.SaveChanges();
    
        // Создаем теперь только деталь после всех проверок
        var safeDetail = new DetailResponseDto
        {
            Name = realDetail.Name,
            Count = realDetail.Count,
            Status = realDetail.Status,
            CreatorId = realDetail.CreatorId,
            Id = realDetail.Id
        };
        
        return safeDetail;
    }
    
    // Получаем все детали
    public List<DetailResponseDto> GetDetailsAll()
    {
        var allDetails = _db.Details
            .Select(d => new DetailResponseDto
            {
                Name = d.Name,
                Count = d.Count,
                Status = d.Status,
                CreatorId = d.CreatorId,
                Id = d.Id
            })
            .ToList();
        return allDetails;
    }
    
    // Получаем одну конкретную деталь
    public DetailResponseDto? GetDetail(int id)
    {
        var detail = _db.Details.Find(id);

        if (detail != null)
        {
            // Создаем теперь только деталь после всех проверок
            var safeDetail = new DetailResponseDto
            {
                Name = detail.Name,
                Count = detail.Count,
                Status = detail.Status,
                CreatorId = detail.CreatorId,
                Id = detail.Id
            };
        
            return safeDetail;
        }
        else
        {
            return null;
        }
    }
}