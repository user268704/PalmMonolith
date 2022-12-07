using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm;

public class SessionHub : Hub
{
    private readonly SessionManager _sessionManager;
    private readonly UserManager<User> _userManager;

    public SessionHub(SessionManager sessionManager, UserManager<User> userManager)
    {
        _sessionManager = sessionManager;
        _userManager = userManager;
    }
    
    public async Task ConfirmSession(List<Question> questions, string sessionId)
    {
        
    }

    public async Task StartSession(string sessionId)
    {
        
    }
    
    public override async Task OnConnectedAsync()
    {

        User user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

        if (await _userManager.IsInRoleAsync(user, "teacher"))
        {
            Context.GetHttpContext().Session.SetString("hostId", Context.ConnectionId);

            return;
        }

        var session = Context.GetHttpContext().Session;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, session.GetString("sessionId"));
        await Clients.Client(session.GetString("hostId")).SendAsync("UserJoined", user.Id, user.UserName);
    }
}