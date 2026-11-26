using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSystem.Controllers.Account
{
    [Authorize(Roles = "Doctor")]
    public class DoctorAccController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Shared/_Layout.cshtml");
        }
    }
}
