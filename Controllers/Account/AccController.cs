using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClinicSystem.Models.ViewModels.AccVM;
using ClinicSystem.Models.Account;
using ClinicSystem.Data;
using System.Text.RegularExpressions;

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
        [ValidateAntiForgeryToken]
        public IActionResult Register(AccRegisterVM vm)
        {
            if (vm.IdType == IdType.NRIC && !Regex.IsMatch(vm.IdNumber, @"^\d{12}$"))
            {
                ModelState.AddModelError("IdNumber", "Invalid NRIC number*");
            }
            else if (vm.IdType == IdType.Passport && !Regex.IsMatch(vm.IdNumber, @"^[A-Z]{2}\d{5}$"))
            {
                ModelState.AddModelError("IdNumber", "Invalid Passport number*");
            }

            // Check if the email is already registered
            if (!string.IsNullOrWhiteSpace(vm.Email))
            {
                var email = vm.Email.Trim();

                if (context.Users.Any(u => u.Email == email))
                {
                    ModelState.AddModelError("Email", "This email is already registered*");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Create a new Patient and User entity
            var user = new User
            {
                FullName = vm.FullName,
                Email = vm.Email
            };

            // Hash the password before saving
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, vm.Password);

            var patient = new Patient
            {
                User = user,
            };

            context.Patients.Add(patient);
            context.SaveChanges();

            return RedirectToAction("Login");
        }

        // GET: /Acc/Login Login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Acc/Login Authenticate user and sign in
        [HttpPost]
        public async Task<IActionResult> Login(AccLoginVM vm)
        {
            // Validate the input model
            var user =  await context.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);
            var hasher = new PasswordHasher<User>();

            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid email*");
            }
            else if (user.AccountStatus != AccountStatus.Active)
            {
                ModelState.AddModelError("Email", "Account is not active*");
            }
            else if (hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Password", "Invalid password*");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }
                

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            /* return user.Role switch
            {
                Role.Admin => RedirectToAction("Index", "AdminAcc"),
                Role.Doctor => RedirectToAction("Index", "DoctorAcc"),
                Role.Patient => RedirectToAction("Index", "PatientAcc"),
                _ => View(vm
            }; */

            return View("~/Views/Shared/_MainMenu.cshtml");
        }
    }
}
