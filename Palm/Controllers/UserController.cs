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
    private readonly IMapper _mapper;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public UserController(UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<User> signInManager,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _mapper = mapper;
    }


    [Authorize]
    [Route("logout")]
    [HttpPost]
    public IActionResult Logout()
    {
        _signInManager.SignOutAsync();
        
        return Ok();
    }

    [Authorize]
    [Route("profile")]
    [HttpGet]
    public IActionResult Profile()
    {
        return Ok();
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

    [Route("/get-me")]
    [HttpGet]
    public async Task<IActionResult> GetMe()
    {
        User user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

        return Ok(user);
    }

#endif
}