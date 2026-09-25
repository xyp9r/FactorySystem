using FluentValidation;
using FactorySystem.Models;
using FluentValidation.Validators;

namespace FactorySystem.Validators;

public class CreateDetailDtoValidator : AbstractValidator<CreateDetailDto>
{
    public CreateDetailDtoValidator()
    {
        // Правилось 1 и 2 : Имя
        RuleFor(x => x.Name)
            .Length(2, 50).NotEmpty().WithMessage("Название детали не может быть пустым");
        
        // Правило 3 : Количество
        RuleFor(x => x.Count)
            .GreaterThanOrEqualTo(1).WithMessage("Количество деталей должно быть минимум 1 шт.");
        
        // Правилось 4 : Статус (пока сделаем простую проверку через кастомное условие)
        RuleFor(x => x.Status)
            .Must(status => status == "В процессе" || status == "Готовая")
            .WithMessage("Статус может быть только в 'В процессе' или 'Готовая'");

    }
}