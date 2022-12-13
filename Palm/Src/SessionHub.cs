using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Palm.Abstractions.Interfaces.Managers;
using Palm.Models.Errors;
using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm;

public class SessionHub : Hub
{
    private readonly IMapper _mapper;
    private readonly ISessionManager _sessionManager;
    private readonly UserManager<User> _userManager;

    public SessionHub(ISessionManager sessionManager, IMapper mapper, UserManager<User> userManager)
    {
        _sessionManager = sessionManager;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task Reply(string sessionId, string questionId, int answerId)
    {
        var session = _sessionManager.GetSession(sessionId);
        var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

        var take = session.Takes.Find(take => take.StudentId == user.Id);
        take.QuestionAnswers.Add(new QuestionAnswer
        {
            AnswerId = answerId,
            QuestionId = questionId
        });

        _sessionManager.AddUpdates(session);
    }

    [Authorize("teacher")]
    public async Task EndSession(string sessionId)
    {
        try
        {
            _sessionManager.EndSession(sessionId);
        }
        catch (ArgumentException e)
        {
            await SendErrorAndAbortAsync(new ErrorResponse
            {
                Error = e.ToString(),
                Message = e.Message
            });

            return;
        }

        await Clients.Group(sessionId).SendAsync("SessionEnded");
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
        var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        var session = _sessionManager.GetSession(sessionId);

        if (!session.Students.Contains(user.Id))
        {
            await SendErrorAndAbortAsync(new ErrorResponse
            {
                Error = "Нет доступа к сессии",
                Message = "Вы не являетесь участником сессии"
            });

            return;
        }

        if (!session.IsConnectAfterStart && IsSessionActive(session))
        {
            await SendErrorAndAbortAsync(new ErrorResponse
            {
                Error = "Нет доступа к сессии",
                Message = "К этой сессии нельзя подключиться после ее начала"
            });
            
            return;
        }
        
        if (!IsSessionActive(session))
        {
            await SendErrorAndAbortAsync(new ErrorResponse
            {
                Error = "Нет доступа к сессии",
                Message = "Сессия не активна"
            });
            
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, session.GroupInfo.GroupName);
        var userRegister = _mapper.Map<UserRegister>(user);

        await Clients.Client(session.GroupInfo.TeacherId)
            .SendAsync("UserJoined", userRegister);
    }

    [Authorize("teacher")]
    public async void ExpelStudent(string sessionId, string studentId)
    {
        var session = _sessionManager.GetSession(sessionId);
        var take = session.Takes.Find(user => user.StudentId == studentId);

        session.Students.Remove(studentId);
        session.Takes.RemoveAll(take => take.StudentId == studentId);

        _sessionManager.UpdateSession(session);
        await Groups.RemoveFromGroupAsync(take.ConnectionId, session.GroupInfo.GroupName);
    }

    public override async Task OnConnectedAsync()
    { 
        User user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

        if (await _userManager.IsInRoleAsync(user, "student"))
        {
            Session? session = _sessionManager.GetSessionsByStudent(user.Id);
            if (user != null && session != null)
            {
                StudentConnection(session, user);

                await Clients.Client(session.GroupInfo.TeacherId)
                    .SendAsync("UserJoined", _mapper.Map<UserRegister>(user));
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var session = _sessionManager.GetSessionByStudentConnectionId(Context.ConnectionId);
        
        // TODO: Сделать нормально
        if (session == null)
        {
            var teacherSession = _sessionManager.GetSessionByTeacherConnectionId(Context.ConnectionId);
            teacherSession.GroupInfo.IsTeacherConnected = false;

            _sessionManager.UpdateSession(teacherSession);
            return;
        }
        
        
        var take = session.Takes.Find(take => take.ConnectionId == Context.ConnectionId);
        
        session.Takes.Remove(take);
        _sessionManager.UpdateSession(session);
        
        await Clients.Client(session.GroupInfo.TeacherId).SendAsync("UserLeft", take.StudentId);
    }

    [Authorize("teacher")]
    public async Task InitialSession(string sessionId)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
        {
            await SendErrorAndAbortAsync(new ErrorResponse
            {
                Error = "Сессии не существует",
                Message = "Сессии с таким идентификатором не существует"
            });
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

        session.GroupInfo = new SessionGroupInfo
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

    private async Task SendErrorAndAbortAsync(ErrorResponse error)
    {
        await Clients.Caller.SendAsync("Error", error);
        
        Context.Abort();
    }

    private async Task SendErrorAndAbortAsync(ErrorResponse error, string connectionId)
    {
        await Clients.Client(connectionId).SendAsync("Error", error);

        Context.Abort();
    }

    private void StudentConnection(Session session, User user)
    {
        var take = session.Takes.Find(prop => prop.StudentId == user.Id);
        if (take == null)
        {
            session.Takes = new();
            session.Takes.Add(new Take
            {
                ConnectionId = Context.ConnectionId,
                StudentId = user.Id,
                TimeStart = TimeOnly.FromDateTime(DateTime.Now)
            });
        }
        else take.ConnectionId = Context.ConnectionId;
        
        _sessionManager.UpdateSession(session);
    }

    private async Task UpdateTakeAsync(Session session, User user)
    {
        Take? take = session.Takes.Find(take => take.StudentId == user.Id);
        if (take == null)
        {
            await SendErrorAndAbortAsync(new ErrorResponse()
            {
                Error = "Ошибка",
                Message = "Вы не подключены к сессии"
            });

            return;
        }

        session.Takes.RemoveAll(take1 => take1.StudentId == user.Id);
        session.Takes.Add(take);
    }
}