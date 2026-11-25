using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSystem.Controllers.Account
{
    [Authorize(Roles = "Admin")]
    public class AdminAccController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Shared/_Layout.cshtml");
        }
    }
}
