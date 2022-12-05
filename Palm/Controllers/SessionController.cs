using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Palm.Infrastructure;
using Palm.Models.Sessions;
using Palm.Models.Sessions.Dto;
using Palm.Models.Users;

namespace Palm.Controllers;

[ApiController]
[Route("api/session")]
public class SessionController : ControllerBase
{
    private readonly SessionManager _sessionManager;
    private readonly UserDataContext _userContext;
    private readonly UserDataContext _db;
    private readonly IMapper _mapper;

    public SessionController(SessionManager sessionManager, UserDataContext userContext, IMapper mapper)
    {
        _sessionManager = sessionManager;
        _userContext = userContext;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Подключение студента к сессии
    /// </summary>
    /// <param name="userId">Id пользователя который будет подключаться</param>
    /// <param name="sessionId">Id сессии к которой будут подключаться</param>
    [Route("join/{sessionId}")]
    [HttpPost]
    public IActionResult Join(Guid userId, string sessionId)
    {
        Session session = _sessionManager.GetSession(sessionId);
        Student student = new()
        {
            Email = "vana@gmail.com",
            Name = "Vana",
            HistorySessions = new List<Session>(),
            Id = Guid.NewGuid()
        };

        try
        {
            _sessionManager.AddStudentToSession(session, student);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        
        return Ok(session);
    }

    /// <summary>
    /// Создаёт новую сессию
    /// </summary>
    /// <param name="sessionDto"></param>
    /// <returns></returns>
    [Route("create")]
    [HttpPost]
    public IActionResult Create(SessionDto sessionDto)
    {
        sessionDto.EndDate = sessionDto.EndDate.ToUniversalTime();
        sessionDto.StartDate = sessionDto.StartDate.ToUniversalTime();
        Session fullSession = _mapper.Map<Session>(sessionDto);
        
        _sessionManager.AddSession(fullSession);

        return Ok();
    }

    /// <summary>
    /// Возвращает сессию по её id
    /// </summary>
    /// <param name="shortId"></param>
    /// <returns></returns>
    [Route("get/{shortId}")]
    [HttpGet]
    public IActionResult GetSession(string shortId)
    {
        Session session = _sessionManager.GetSession(shortId);
        
        return Ok(session);
    }
    
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
    
    /// <summary>
    /// Удаление сессии
    /// </summary>
    /// <param name="shortId"></param>
    /// <returns></returns>
    [Route("remove/{shortId}")]
    [HttpPost]
    public IActionResult RemoveSession(string shortId)
    {
        _sessionManager.RemoveSession(shortId);
        
        return Ok();
    }
}