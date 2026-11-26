using ClinicSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSystem.Controllers.Account
{
    public class ProfileController(ClinicDbContext context) : Controller
    {
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Index(string? id)
        {
            var patientInfo = await context.Users.FindAsync(id);

            if (patientInfo == null)
            {
                return NotFound();
            }

            return View(patientInfo);
        }
    }
}
