using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Palm.Models.Users;

namespace Palm.Controllers;

[ApiController]
[Route("api/users/")]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }
    
    
    [Route("get/{userId}")]
    [HttpGet]
    public IActionResult GetUser(Guid userId)
    {
        return Ok("Чувак, ты авторизован");
    }

#if DEBUG

    [Authorize]
    [Route("make-me-boss")]
    [HttpPost]
    public async Task<IActionResult> MakeMeBoss(string rank)
    {
        if (!await _roleManager.RoleExistsAsync(rank)) 
            await _roleManager.CreateAsync(new IdentityRole(rank));

        await _roleManager.CreateAsync(new IdentityRole("student"));
        await _roleManager.CreateAsync(new IdentityRole("teacher"));

        User me = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
        await _userManager.AddToRoleAsync(me, rank);

        return Ok("Ты теперь босс, бро");
    }
    
#endif


    /*
    public IActionResult UpdateUser()
    {
        
    }
    */
    
    /*
    public IActionResult DeleteUser()
    {
        
    }
*/
}