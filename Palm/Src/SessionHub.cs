using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm;

public class SessionHub : Hub
{
    private readonly SessionManager _sessionManager;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public SessionHub(SessionManager sessionManager, IMapper mapper, UserManager<User> userManager)
    {
        _sessionManager = sessionManager;
        _mapper = mapper;
        _userManager = userManager;
    }

    public void Reply(string sessionId, string questionId, string answerId)
    {
        
    }

    public void StartSession(string sessionId)
    {
        Session session = _sessionManager.GetSession(sessionId);

        Clients.Group(session.GroupInfo.GroupName).SendAsync("StartSession");
    }

    public async Task JoinSession(string sessionId)
    {
        User user = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        Session session = _sessionManager.GetSession(sessionId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, session.GroupInfo.GroupName);
        UserRegister userRegister = _mapper.Map<UserRegister>(user);
        
        await Clients.Client(session.GroupInfo.TeacherId).SendAsync("UserJoined", userRegister);
    }
    
    [Authorize("teacher")]
    public async Task InitialSession(string sessionId)
    {
        // User user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

        Session session = _sessionManager.GetSession(sessionId);
        session.GroupInfo = new()
        {
            GroupName = sessionId,
            TeacherId = Context.ConnectionId
        };
        
        _sessionManager.UpdateSession(session);
    }
}