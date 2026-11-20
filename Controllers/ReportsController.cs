using Microsoft.AspNetCore.Mvc;

namespace ClinicSystem.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
