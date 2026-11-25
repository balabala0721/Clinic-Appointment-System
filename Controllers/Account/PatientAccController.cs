using ClinicSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSystem.Controllers.Account
{
    [Authorize(Roles = Roles.Patient)]
    public class PatientAccController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
