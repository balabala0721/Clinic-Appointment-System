using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClinicSystem.Models.ViewModels.AccVM;
using ClinicSystem.Models.Account;
using ClinicSystem.Data;

namespace ClinicSystem.Controllers.Account
{
    public class AccController(ClinicDbContext context) : Controller
    {
        // GET: /Acc/Register Register page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Acc/Register Register a new account
        [HttpPost]
        public IActionResult Register(AccRegisterVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var patient = new Patient
            {
                User = new User
                {
                    FullName = vm.FullName,
                    Email = vm.Email,
                    PhoneNo = vm.PhoneNo
                },
                
                IdType = vm.IdType,
                Gender = vm.Gender,
                DateOfBirth = vm.DateOfBirth
            };

            context.Patients.Add(patient);
            context.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
    }
}
