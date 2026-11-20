using Microsoft.AspNetCore.Mvc;

namespace ClinicSystem.Controllers
{
    public class DrugsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
