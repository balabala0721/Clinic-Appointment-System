using ClinicSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSystem.Controllers.Account
{
    /// <summary>Staff landing page: the front desk (Views/StaffAcc/Index.cshtml).</summary>
    [Authorize(Roles = Roles.Staff)]
    public class StaffAccController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
