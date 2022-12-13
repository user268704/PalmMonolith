using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Palm.Abstractions.Interfaces.Managers;
using Palm.Caching;
using Palm.Exceptions;
using Palm.Infrastructure;
using Palm.Models.Errors;
using Palm.Models.Sessions;
using Palm.Models.Sessions.Dto;
using Palm.Models.Users;
using Palm.Validator.Validators;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace Palm.Controllers;

[ApiController]
[Route("api/session")]
public class SessionController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly QuestionsCaching _questionsCaching;
    private readonly SessionDataContext _sessionDataContext;
    private readonly ISessionManager _sessionManager;
    private readonly UserManager<User> _userManager;

    public SessionController(ISessionManager sessionManager, SessionDataContext sessionDataContext,
        UserManager<User> userManager, IMapper mapper)
    {
        _sessionManager = sessionManager;
        _sessionDataContext = sessionDataContext;
        _questionsCaching = new QuestionsCaching();
        _userManager = userManager;
        _mapper = mapper;
    }

    /// <summary>
    ///     Подключение студента к сессии
    /// </summary>
    /// <param name="isAuthUser">
    ///     Флаг который сообщает авторизован ли пользователь, если да то добавлять его по аккаунту, если
    ///     нет то редирект на страницу регистрации
    /// </param>
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

        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
            return BadRequest(new ErrorResponse
            {
                Error = "Такой сессии не существует",
                Message = "Сессия не найдена"
            });

        var userName = HttpContext.User.Identity.Name;

        var user = await _userManager.FindByNameAsync(userName);

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
        }
        catch (ArgumentException)
        {
            // Просто редиректим в любом случае, если подключен то может пройти,
            // если нет то выше подключается
        }

        return RedirectToAction("Index", "SessionViews", new
        {
            sessionId
        });
    }

    /// <summary>
    ///     Создаёт новую сессию
    /// </summary>
    /// <param name="sessionDto"></param>
    /// <returns></returns>
    /// TODO: Какого-то хера тут я не прохожу проверку будучи учителем 🤦‍
    /// а в другом эндпоинте с таким же атрибутом и в этом же аккаунте всё хорошо
    [Authorize("teacher")]
    [Route("create")]
    [HttpPost]
    public async Task<IActionResult> Create(SessionDto sessionDto)
    {
        var validator = new SessionCreateValidator();

        var validationResult = validator.Validate(sessionDto);
        if (!validationResult.IsValid) 
            return BadRequest(new ErrorResponse
            {
                Error = "Возникли ошибки при создании сессии",
                Message = string.Join(", ", validationResult.Errors.Select(error => error.ErrorMessage))
            });

        sessionDto.EndDate = sessionDto.EndDate.ToUniversalTime();
        sessionDto.StartDate = sessionDto.StartDate.ToUniversalTime();

        var fullQuestions = _mapper.Map<IEnumerable<Question>>((ICollection<QuestionUpdateDto>) sessionDto.Questions);
        var fullSession = _mapper.Map<Session>(sessionDto);

        fullSession.Questions = fullQuestions
            .Select(question => question.Id.ToString())
            .ToList();
        
        var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
        fullSession.HostId = Guid.Parse(user.Id);

        _sessionManager.CreateSession(fullSession);

        // Здесь устанавливается ShortId потому что в CreateSession он устанавливается к сессии по ссылке
        _questionsCaching.AddQuestions(fullQuestions.ToList(), fullSession.ShortId);
        
        // Вопросы добавляем именно тут а не при создании, потому что
        // благодаря методу выше они уже проинициализированы и записаны в Redis
        // такие какие они и будут
        _sessionManager.AddQuestions(fullQuestions.ToList(), fullSession);

        return Ok();
    }

    /// <summary>
    /// Возвращает все сессии учителя
    /// </summary>
    /// <returns></returns>
    [Authorize("teacher")]
    [Route("get/all-my")]
    [HttpGet]
    public async Task<IActionResult> GetAllTeacherSessions()
    {
        var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
        if (user == null)
            return BadRequest(new ErrorResponse
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
    ///     Здесь можно обновить сессию, добавить вопросы, изменить название или дату окончания
    /// </summary>
    /// <param name="session"></param>
    /// <param name="sessionId">Id сессии которую надо изменить</param>
    [Route("update/{sessionId}")]
    [HttpPut]
    public IActionResult UpdateSession(SessionUpdateDto session, string sessionId)
    {
        List<string> questionsId = new();

        var fullQuestions = _mapper.Map<IEnumerable<Question>>(session.Questions);
        try
        {
            if (session.Questions != null)
                questionsId = _questionsCaching.AddQuestions(fullQuestions.ToList(), sessionId);
        }
        catch (NotFoundException e)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Сессия не найдена",
                Message = e.Message
            });
        }

        // BUG: При добавлении первых вопросов, мапинг работает не правильно, и мапит всё к строке
        var sessionToUpdate = _mapper.Map<Session>(session);
        sessionToUpdate.ShortId = sessionId;

        foreach (string questionId in questionsId)
            sessionToUpdate.Questions.Add(questionId);

        _sessionManager.AddUpdates(sessionToUpdate);

        return Ok();
    }

    [Authorize("teacher")]
    [Route("change/questions/{sessionId}")]
    [HttpPut]
    public IActionResult ChangeQuestions([FromQuery] List<string> questions, [FromBody] List<Question> changed,
        string sessionId)
    {
        return StatusCode(501);
    }

    /// <summary>
    ///     Возвращает сессию по её id
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
            var session = _sessionManager.GetSession(shortId);
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

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

    /// <summary>
    /// Возвращает все вопросы сессии
    /// </summary>
    /// <param name="sessionId">Id сессии</param>
    [Route("get/questions/{sessionId}")]
    [HttpGet]
    public IActionResult GetQuestions(string sessionId)
    {
        Session? session = _sessionManager.GetSession(sessionId);
        if (session == null)
            return BadRequest(new ErrorResponse
            {
                Error = "Сессия не найдена",
                Message = "Сессия с таким id не найдена"
            });

        try
        {
            var questions = _questionsCaching.GetQuestionsFromSession(sessionId);

            return Ok(questions);
        }
        catch (NotFoundException e)
        {
            return BadRequest(new ErrorResponse
            {
                Error = e.WhatNotHound + " не найден",
                Message = e.Message
            });
        }
    }

#if DEBUG
    /// <summary>
    ///     Возвращает все существующие сессии
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
    ///     Удаление сессии
    /// </summary>
    [Authorize("teacher")]
    [Route("remove/{shortId}")]
    [HttpPost]
    public async Task<IActionResult> RemoveSession(string shortId)
    {
        var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
        var session = _sessionManager.GetSession(shortId);

        if (user.Id == session.HostId.ToString())
        {
            _sessionManager.RemoveSession(shortId);
            return Ok();
        }

        return Forbid();
    }
}