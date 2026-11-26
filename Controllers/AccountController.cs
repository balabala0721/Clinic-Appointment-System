using System.Security.Claims;
using ClinicSystem.Data;
using ClinicSystem.Models.Entities;
using ClinicSystem.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Controllers;

/// <summary>
/// TEMPORARY test login - click a user to sign in as them, no password.
/// Every action returns 404 outside Development, so this can't be used on a real server.
/// Replace with the Auth module's real login. The important part to copy is SignInAsUserAsync:
/// the claims it sets are what CurrentUserService reads.
/// </summary>
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AccountController(ClinicDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        if (!_env.IsDevelopment()) return NotFound();

        // If the tables don't exist yet, explain what to do instead of crashing.
        if (!await _db.Database.CanConnectAsync() || (await _db.Database.GetPendingMigrationsAsync()).Any()
            || !_db.Database.GetMigrations().Any())
        {
            ViewBag.DbNotReady = true;
            return View(new List<DevLoginUser>());
        }

        // Show each user with their display name (FullName lives on User; fall back to email if blank)
        var users = await _db.Users.Where(u => u.AccountStatus == AccountStatus.Active).OrderBy(u => u.Role).ThenBy(u => u.Email)
                                   .AsNoTracking().ToListAsync();

        var model = users.Select(u => new DevLoginUser(
            u.UserId, u.Role.ToString(), u.Email,
            string.IsNullOrWhiteSpace(u.FullName) ? u.Email : u.FullName)).ToList();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(int userId)
    {
        if (!_env.IsDevelopment()) return NotFound();

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId && u.AccountStatus == AccountStatus.Active);
        if (user == null) return RedirectToAction(nameof(Login));

        await SignInAsUserAsync(user);
        return RedirectToHome(user.Role.ToString());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetDemoData()
    {
        if (!_env.IsDevelopment()) return NotFound();
        try
        {
            await DevDataSeeder.ResetAppointmentsAsync(_db);
            TempData["Success"] = "Demo appointments and leave were reset.";
        }
        catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
        {
            // e.g. a teammate's MedicalVisits rows still point at an appointment
            TempData["Error"] = "Reset failed - other tables still reference these appointments. Recreate the database instead.";
        }
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    // ------------------------------------------------------------------ helpers

    /// <summary>This is the bit the real login must also do after checking the password.</summary>
    private async Task SignInAsUserAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),   // read by CurrentUserService.UserId
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())                           // used by [Authorize(Roles = ...)]
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    private IActionResult RedirectToHome(string role) => role switch
    {
        Roles.Patient => RedirectToAction("Book", "Appointments"),
        Roles.Doctor => RedirectToAction("Mine", "DoctorSchedules"),
        _ => RedirectToAction("Manage", "Appointments")              // Staff / Admin
    };
}

public record DevLoginUser(int UserId, string Role, string Email, string DisplayName);
