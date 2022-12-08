using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Palm.Controllers;

[Route("session")]
public class SessionViewsController : Controller
{
    
    [Authorize("student")]
    [Route("{sessionId}")]
    [HttpGet]
    public IActionResult Index(string sessionId)
    {
        ViewBag.sessionId = sessionId;

        return View("~/Views/Session/Index.cshtml");
    }

    [Authorize("teacher")]
    [Route("{sessionId}/manage")]
    [HttpGet]
    public IActionResult Manage(string sessionId)
    {
        ViewBag.sessionId = sessionId;
        return View("~/Views/Session/Manage.cshtml", sessionId);
    }
}