using FluentValidation;
using Palm.Models.Sessions.Dto;

namespace Palm.Validator.Validators;

public class SessionCreateValidator : AbstractValidator<SessionDto>
{
    public SessionCreateValidator()
    {
        RuleFor(session => session.EndDate > session.StartDate)
            .Equal(true)
            .WithMessage("Сессия не может кончаться раньше чем началась");

        RuleFor(session => session.EndDate)
            .NotEqual(DateTime.MinValue)
            .WithMessage("Время окончания сессии не может быть пустой");

        RuleFor(session => session.Title)
            .NotEmpty()
            .WithMessage("Название сессии не может быть пустым");
    }
}