using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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

    [Route("login-provider")]
    [HttpPost]
    public IActionResult ExternalLogin()
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Home", new
        {
            ReturnUrl = Url.Action("Index", "Home")
        });

        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);

        return new ChallengeResult("Google", properties);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    { 
        returnUrl = returnUrl ?? Url.Content("~/");

        // Если произошла ошибка во время внешней авторизации
        if (remoteError != null) 
        {
            return BadRequest("Ошибочка: " + remoteError);
        }

            
        // Получаем информацию о пользователе из внешней авторизации
        // Если пользователь не авторизован, то возвращаемся на страницу логина
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) 
        {
            return BadRequest("Ошибка при получении информации о пользователе");
        }
        
        // If the user already has a login (i.e if there is a record in AspNetUserLogins
        // table) then sign-in the user with this external login provider
        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, 
            info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        
        if (signInResult.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }
        // If there is no record in AspNetUserLogins table, the user may not have
        // a local account
        else
        {
            // Get the email claim value
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email != null)
            {
                // Create a new user without password if we do not have a user already
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };

                    await _userManager.CreateAsync(user);
                }

                // Add a login (i.e insert a row for the user in AspNetUserLogins table)
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);

                return LocalRedirect(returnUrl);
            }
            
            return BadRequest(new
            {
                Error = $"Email claim not received from: {info.LoginProvider}",
                Error2 = "Please contact support on Pragim@PragimTech.com"
            });
        }
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