using Microsoft.AspNetCore.Mvc;

namespace Palm.Controllers;

public class ProfileController : Controller
{
    // GET
    public IActionResult Index()
    {
        return Ok();
    }
}