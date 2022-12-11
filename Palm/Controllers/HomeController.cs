using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Palm.Models.Errors;
using Palm.Models.Users;

namespace Palm.Controllers;

public class HomeController : Controller
{
    private readonly IMapper _mapper;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public HomeController(SignInManager<User> signInManager, UserManager<User> userManager, IMapper mapper)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _mapper = mapper;
    }

    [Route("/")]
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [Route("login")]
    [HttpGet]
    public IActionResult LoginView(string? fromSession)
    {
        ViewData["fromSession"] = fromSession;

        return View("Login");
    }

    [Route("login")]
    [HttpPost]
    public async Task<IActionResult> Login(UserRegister user, string? fromSession)
    {
        var fullUser = _mapper.Map<User>(user);
        var userFromDb = await _userManager.FindByNameAsync(fullUser.UserName);

        if (userFromDb != null && await _userManager.CheckPasswordAsync(userFromDb, user.Password))
        {
            await _signInManager.SignInAsync(userFromDb, false);
            if (string.IsNullOrEmpty(fromSession))
                return Ok();

            return RedirectToAction("Join", "Session", new
            {
                sessionId = fromSession,
                isAuthUser = true
            });
        }

        return BadRequest(new ErrorResponse
        {
            Error = "Неверный логин или пароль",
            Message = "Попробуйте ещё раз"
        });
    }

    [Route("register")]
    [HttpGet]
    public IActionResult RegisterView(string? fromSession)
    {
        ViewData["fromSession"] = fromSession;

        return View("Register");
    }

    [Route("register")]
    [HttpPost]
    public async Task<IActionResult> Register(UserRegister user, bool isTeacher, string? fromSession)
    {
        var fullUser = _mapper.Map<User>(user);

        var createResult = await _userManager.CreateAsync(fullUser, user.Password);
        if (createResult.Succeeded)
        {
            if (isTeacher) await _userManager.AddToRoleAsync(fullUser, "teacher");
            else await _userManager.AddToRoleAsync(fullUser, "student");

            await _signInManager.SignInAsync(fullUser, true);
            if (string.IsNullOrEmpty(fromSession))
                return RedirectToPage("/profile");


            return RedirectToAction("Join", "Session", new
            {
                sessionId = fromSession,
                isAuthUser = true
            });
        }

        return BadRequest(new ErrorResponse
        {
            Error = string.Join(", ", createResult.Errors.Select(x => x.Description)),
            Message = "Ошибка, попробуйте ещё раз"
        });
    }
}