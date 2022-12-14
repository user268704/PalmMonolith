using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Palm.Abstractions.Interfaces.Managers;
using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm.Validator.Validators;

public class SessionJoinValidator : AbstractValidator<Session>
{
    /// <summary>
    ///     Валидатор для присоединения в сессию
    /// </summary>
    public SessionJoinValidator(ISessionManager sessionManager, UserManager<User> userManager,
        Session sessionToJoin, User student)
    {
        Session? session = sessionManager.GetSession(sessionToJoin.ShortId);
        
        RuleFor(x => session)
            .NotNull()
            .WithMessage("Сессия не найдена");

        
        //RuleFor()
    }

    private bool IsUserInSession(User user, Session session)
    {
        return session.Students.Contains(user.Id);
    }
}