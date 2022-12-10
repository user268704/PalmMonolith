using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Palm.Models.Errors;
using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm;

public class SessionHub : Hub
{
    private readonly IMapper _mapper;
    private readonly SessionManager _sessionManager;
    private readonly UserManager<User> _userManager;

    public SessionHub(SessionManager sessionManager, IMapper mapper, UserManager<User> userManager)
    {
        _sessionManager = sessionManager;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task Reply(string sessionId, string questionId, int answerId)
    {
        Session? session = _sessionManager.GetSession(sessionId);
        User user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

        Take take = session.Takes.Find(take => take.StudentId == user.Id);
        take.QuestionAnswers.Add(new()
        {
            AnswerId = answerId,
            QuestionId = questionId
        });
        
        _sessionManager.UpdateSession(session);
    }

    [Authorize("teacher")]
    public void EndSession(string sessionId)
    {
        try
        {
            _sessionManager.EndSession(sessionId);
        }
        catch (ArgumentException e)
        {
            Clients.Caller.SendAsync("Error", new ErrorResponse()
            {
                Error = e.ToString(),
                Message = e.Message
            });
            
            return;
        }

        Clients.Group(sessionId).SendAsync("SessionEnded");
    }

    public void StartSession(string sessionId)
    {
        Clients.Group(sessionId).SendAsync("StartSession");
    }

    [Authorize("teacher")]
    public void StopSession(string sessionId)
    {
        Clients.Group(sessionId).SendAsync("StopSession");
    }

    [Authorize("student")]
    public async Task JoinSession(string sessionId)
    {
        User user = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        Session session = _sessionManager.GetSession(sessionId);
        
        if (!session.Students.Contains(user.Id))
        {
            await Clients.Caller.SendAsync("Error", new ErrorResponse()
            {
                Error = "Нет доступа к сессии",
                Message = "Вы не являетесь участником сессии"
            });
            
            Context.Abort();
            return;
        }

        if (!IsSessionActive(session))
        {
            await Clients.Caller.SendAsync("Error", new ErrorResponse()
            {
                Error = "Нет доступа к сессии",
                Message = "Сессия не активна"
            });
            
            Context.Abort();
            return;
        }
        
        Take? take = session.Takes.Find(prop => prop.StudentId == user.Id);
        if (take == null)
        {
            session.Takes.Add(new Take()
            {
                ConnectionId = Context.ConnectionId,
                StudentId = user.Id,
                TimeStart = TimeOnly.FromDateTime(DateTime.Now)
            });
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, session.GroupInfo.GroupName);
        UserRegister userRegister = _mapper.Map<UserRegister>(user);
        
        _sessionManager.UpdateSession(session);
        
        await Clients.Client(session.GroupInfo.TeacherId).SendAsync("UserJoined", userRegister);
    }

    [Authorize("teacher")]
    public async void ExpelStudent(string sessionId, string studentId)
    {
        Session session = _sessionManager.GetSession(sessionId);
        Take take = session.Takes.Find(user => user.StudentId == studentId);
        
        session.Students.Remove(studentId);
        session.Takes.RemoveAll(take => take.StudentId == studentId);
        
        _sessionManager.UpdateSession(session);
        await Groups.RemoveFromGroupAsync(take.ConnectionId, session.GroupInfo.GroupName);
    }

    /*
    public override async Task OnConnectedAsync()
    {
        Session? sessionIfIsStudent = _sessionManager.GetSessionByStudentConnectionId(Context.ConnectionId);
        Session? sessionIfIsTeacher;

        if (Context.User == null)
        {
            Clients.Caller.SendAsync("Error", new ErrorResponse
            {
                Error = "Вы не авторизованы",
                Message = "Пожалуйста, войдите в свой аккаунт или зарегистрируйтесь"
            });

            Context.Abort();
            return;
        }
        
        User? user = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        if (user == null)
        {
            await Clients.Caller.SendAsync("Error", new ErrorResponse()
            {
                Error = "Ошибка авторизации",
                Message = "Пользователь не авторизован"
            });
            
            Context.Abort();
            return;
        }

        // Если заходит учитель
        if (await _userManager.IsInRoleAsync(user, "teacher") &&
            sessionIfIsTeacher.HostId == Guid.Parse((ReadOnlySpan<char>)user.Id))
            return;

        // Сессия может быть не найдена, если ученик не зашел в сессию
        if (session == null)
        {
            Clients.Caller.SendAsync("Error", new ErrorResponse()
            {
                Error = "Нет доступа к сессии",
                Message = "Вы не являетесь участником сессии"
            });

            var f = Context.UserIdentifier;
            
            Context.Abort();
        }
    }
    */

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Session? session = _sessionManager.GetSessionByStudentConnectionId(Context.ConnectionId);
        Take take = session.Takes.Find(take => take.ConnectionId == Context.ConnectionId);
        
        session.Takes.Remove(take);
        _sessionManager.UpdateSession(session);
        await Clients.Client(session.GroupInfo.TeacherId).SendAsync("UserLeft", take.StudentId);
    }

    [Authorize("teacher")]
    public void InitialSession(string sessionId)
    {
        Session? session = _sessionManager.GetSession(sessionId);
        if (session == null)
        {
            Clients.Caller.SendAsync("Error", new ErrorResponse
            {
                Error = "Сессии не существует",
                Message = "Сессии с таким идентификатором не существует"
            });
            
            Context.Abort();
            return;
        }
        
        // Обновляем Id учителя если он переподключился, а сессия уже идёт
        if (!string.IsNullOrEmpty(session.GroupInfo.GroupName))
        {
            session.GroupInfo.IsTeacherConnected = true;
            session.GroupInfo.TeacherId = Context.ConnectionId;
            _sessionManager.UpdateSession(session);
            
            return;
        }

        session.GroupInfo = new()
        {
            IsTeacherConnected = true,
            GroupName = sessionId,
            TeacherId = Context.ConnectionId
        };
        
        _sessionManager.UpdateSession(session);
    }

    private bool IsSessionActive(Session session)
    {
        return session.GroupInfo.IsTeacherConnected && 
               !string.IsNullOrEmpty(session.GroupInfo.TeacherId) && 
               !string.IsNullOrEmpty(session.GroupInfo.GroupName);
    }
}