using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Palm.Models.Errors;
using Palm.Models.Users;

namespace Palm.Controllers;

public class HomeController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public HomeController(SignInManager<User> signInManager, UserManager<User> userManager, IMapper mapper)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _mapper = mapper;
    }
    
    [Route("login")]
    [HttpPost]
    public async Task<IActionResult> Login(UserRegister user)
    {
        User fullUser = _mapper.Map<User>(user);
        User? userFromDb = await _userManager.FindByNameAsync(fullUser.UserName);
        
        if (userFromDb != null && await _userManager.CheckPasswordAsync(userFromDb, user.Password))
        {
            await _signInManager.SignInAsync(userFromDb, false);
            return Ok();
        }

        return BadRequest(new ErrorResponse
        {
            Error = "Неверный логин или пароль",
            Message = "Попробуйте ещё раз"
        });
    }

    [Authorize]
    [Route("logout")]
    [HttpPost]
    public IActionResult Logout()
    {
        _signInManager.SignOutAsync();
        
        return Ok();
    }

    [Route("register")]
    [HttpPost]
    public async Task<IActionResult> Register(UserRegister user)
    {
        User fullUser = _mapper.Map<User>(user);

        // TODO: Добавить уникальность почты
        var createResult = await _userManager.CreateAsync(fullUser, user.Password);
        if (createResult.Succeeded)
        {
            User? userSignIn = _userManager.FindByEmailAsync(fullUser.Email).Result;
            
            // TODO: Проверить на null
            await _signInManager.SignInAsync(userSignIn, true);
            
            return Ok();
        }
        
        return BadRequest(new ErrorResponse
        {
            Error = string.Join(", ", createResult.Errors.Select(x => x.Description)),
            Message = "Ошибка, попробуйте ещё раз"
        });
    }
}