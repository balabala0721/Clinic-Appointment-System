using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSystem.Controllers;

/// <summary>
/// Friendly error pages. Program.cs re-executes here for 404 and other status codes
/// (UseStatusCodePagesWithReExecute) and, in production, for unhandled errors (500).
/// </summary>
[AllowAnonymous]
public class StatusController : Controller
{
    [Route("Status/{code:int}")]
    public IActionResult Index(int code)
    {
        Response.StatusCode = code;
        return View(code);
    }
}
