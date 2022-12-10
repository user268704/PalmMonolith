using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Palm.Data.Implementations;
using Palm.Models.Errors;
using Palm.Models.Sessions;
using Palm.Models.Sessions.Dto;
using Palm.Models.Users;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace Palm.Controllers;

[ApiController]
[Route("api/session")]
public class SessionController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly QuestionsCaching _questionsCaching;
    private readonly SessionManager _sessionManager;
    private readonly UserManager<User> _userManager;

    public SessionController(SessionManager sessionManager, /*QuestionsCaching questionsCaching,*/ UserManager<User> userManager, IMapper mapper)
    {
        _sessionManager = sessionManager;
        _questionsCaching = new QuestionsCaching();
        _userManager = userManager;
        _mapper = mapper;
    }

    /// <summary>
    /// Подключение студента к сессии
    /// </summary>
    /// <param name="isAuthUser">Флаг который сообщает авторизован ли пользователь, если да то добавлять его по аккаунту, если нет то редирект на страницу регистрации</param>
    /// <param name="sessionId">Id сессии к которой будут подключаться</param>
    /*[Authorize("student")]*/
    [Route("join/{sessionId}")]
    [HttpGet]
    public async Task<IActionResult> Join(bool isAuthUser, string sessionId)
    {
        if (!isAuthUser)
            return RedirectToAction("LoginView", "Home", new
            {
                fromSession = sessionId
            });

        Session? session = _sessionManager.GetSession(sessionId);
        if (session == null)
            return BadRequest(new ErrorResponse()
            {
                Error = "Такой сессии не существует",
                Message = "Сессия не найдена"
            });
        
        var userName = HttpContext.User.Identity.Name;
        
        User? user = await _userManager.FindByNameAsync(userName);
        
        if (user == null)
            return BadRequest(new ErrorResponse
            {
                Error = "Пользователь не найден",
                Message = "Пользователь с таким Id не найден, проверьте правильность введенных данных"
            });

        try
        {
            // ArgumentException если студент уже подключен к сессии
            _sessionManager.AddStudentToSession(session, user);
            HttpContext.Session.SetString("sessionId", sessionId);
        }
        catch (ArgumentException e)
        { }
        
        return RedirectToAction("Index", "SessionViews", new
        {
            sessionId = sessionId
        });
    }

    /// <summary>
    /// Создаёт новую сессию
    /// </summary>
    /// <param name="sessionDto"></param>
    /// <returns></returns>
    /// TODO: Какого-то хера тут я не прохожу проверку будучи учителем 🤦‍
    /// а в другом эндпоинте с таким же атрибутом и в этом же аккаунте всё хорошо
    [Authorize("teacher")]
    [Route("create")]
    [HttpPost]
    public IActionResult Create(SessionDto sessionDto)
    {
        if (!_sessionManager.CheckValid(sessionDto))
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Неверные данные",
                Message = "Проверьте правильность введенных данных"
            });
        }
        
        sessionDto.EndDate = sessionDto.EndDate.ToUniversalTime();
        sessionDto.StartDate = sessionDto.StartDate.ToUniversalTime();
        Session fullSession = _mapper.Map<Session>(sessionDto);
        
        User user = _userManager.FindByNameAsync(HttpContext.User.Identity.Name).Result;
        fullSession.HostId = Guid.Parse(user.Id);
        
        _sessionManager.AddSession(fullSession);

        return Ok();
    }

    [Authorize("teacher")]
    [Route("get/all-my")]
    [HttpGet]
    public async Task<IActionResult> GetAllTeacherSessions()
    {
        User? user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
        if (user == null)
            return BadRequest(new ErrorResponse()
            {
                Error = "Пользователь не найден",
                Message = "Запрашиваемый пользователь не зарегистрирован в системе"
            });

        var sessions = _sessionManager.GetAllSessions();
        var allMySessionResult = sessions
            .Where(prop => prop.HostId.ToString() == user.Id);
        
        return Ok(allMySessionResult);
    }

    /// <summary>
    /// Здесь можно обновить сессию, добавить вопросы, изменить название или дату окончания 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="sessionId">Id сессии которую надо изменить</param>
    [Route("update/{sessionId}")]
    [HttpPut]
    public IActionResult UpdateSession(SessionUpdateDto session, string sessionId)
    {
        List<string> questionsId = new();
        
        IEnumerable<Question> fullQuestions = _mapper.Map<IEnumerable<Question>>(session.Questions);
        try
        {
            if (session.Questions != null)
                questionsId = _questionsCaching.AddQuestions(fullQuestions.ToList(), sessionId);
        }
        catch (ArgumentException e)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Сессия не найдена",
                Message = e.Message
            });
        }

        // BUG: При добавлении первых вопросов, мапинг работает не правильно, и мапит всё к строке
        Session sessionToUpdate = _mapper.Map<Session>(session);
        sessionToUpdate.ShortId = sessionId;
        
        foreach (string question in questionsId) 
            sessionToUpdate.Questions.Add(question);

        _sessionManager.UpdateSession(sessionToUpdate);
        
        return Ok();
    }

    [Authorize("teacher")]
    [Route("change/questions/{sessionId}")]
    [HttpPut]
    public IActionResult ChangeQuestions([FromQuery] List<string> questions, [FromBody] List<Question> changed, string sessionId)
    {
        return StatusCode(501);
    }

    /// <summary>
    /// Возвращает сессию по её id
    /// </summary>
    /// <param name="shortId"></param>
    /// <returns></returns>
    [Authorize("teacher")]
    [Route("get/{shortId}")]
    [HttpGet]
    public async Task<IActionResult> GetSession(string shortId)
    {
        try
        {
            Session session = _sessionManager.GetSession(shortId);
            User user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            
            if (session.HostId.ToString() == user.Id)
                return Ok(session);

            return Forbid();
        }
        catch (ArgumentException e)
        {
            return BadRequest(new ErrorResponse
            {
                Error = e.Message,
                Message = "Сессия не найдена"
            });
        }
    }

#if DEBUG
    /// <summary>
    /// Возвращает все существующие сессии
    /// </summary>
    /// <returns></returns>
    [Route("get/all")]
    [HttpGet]
    public IActionResult GetSessions()
    {
        return Ok(_sessionManager.GetAllSessions());
    }
#endif

    /// <summary>
    /// Удаление сессии
    /// </summary>
    /// <param name="shortId"></param>
    /// <returns></returns>
    [Authorize("teacher")]
    [Route("remove/{shortId}")]
    [HttpPost]
    public async Task<IActionResult> RemoveSession(string shortId)
    {
        User user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
        Session session = _sessionManager.GetSession(shortId);

        if (user.Id == session.HostId.ToString())
        {
            _sessionManager.RemoveSession(shortId);
            return Ok();
        }

        return Forbid();
    }
}