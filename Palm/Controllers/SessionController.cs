using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Palm.Cash;
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
    private readonly SessionManager _sessionManager;
    private readonly QuestionsCaching _questionsCaching;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

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

        Session session = _sessionManager.GetSession(sessionId);
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
            _sessionManager.AddStudentToSession(session, user);
            HttpContext.Session.SetString("sessionId", sessionId);
            
            return RedirectToAction("Index", "SessionViews", new
            {
                sessionId = sessionId
            });
        }
        catch (ArgumentException e)
        {
            //TODO: ПОЧИНИТЬ
            return RedirectToAction("Index", "SessionViews", new
            {
                sessionId = sessionId
            });
        }
    }

    /// <summary>
    /// Создаёт новую сессию
    /// </summary>
    /// <param name="sessionDto"></param>
    /// <returns></returns>
    /// TODO: Добавить проверку на то что пользователь является преподавателем
    /*[Authorize("teacher")]*/
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

    /// <summary>
    /// Здесь можно обновить сессию, добавить вопросы, изменить название или дату окончания 
    /// </summary>
    /// <param name="session">
    /// Все обновления. Вопросы которые будут тут получены,
    /// перезапишут все существующие, поэтому надо передавать всё старое вместе с новым
    /// </param>
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

    /// <summary>
    /// Возвращает сессию по её id
    /// </summary>
    /// <param name="shortId"></param>
    /// <returns></returns>
    /*
    [Authorize("sessionOwner")]
    */
    [Route("get/{shortId}")]
    [HttpGet]
    public IActionResult GetSession(string shortId)
    {
        
        // TODO: Сделать что бы возвращалась сессия только если пользователь является её владельцем
        try
        {
            Session session = _sessionManager.GetSession(shortId);
            return Ok(session);
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
    [Route("remove/{shortId}")]
    [HttpPost]
    public IActionResult RemoveSession(string shortId)
    {
        // TODO: Сделать что бы удалялась сессия только если пользователь является её владельцем
        _sessionManager.RemoveSession(shortId);
        
        return Ok();
    }
}