using System.Security.Claims;
using ClinicSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Services;

/// <summary>
/// Reads the logged-in user from the auth cookie and looks up their
/// PatientId / DoctorId.
///
/// DEPENDENCY: the login code (Auth module) must put the UserId into the
/// ClaimTypes.NameIdentifier claim and the role into ClaimTypes.Role when
/// signing in. Agree this with whoever builds login.
/// </summary>
public class CurrentUserService
{
    private readonly IHttpContextAccessor _http;
    private readonly ClinicDbContext _db;

    public CurrentUserService(IHttpContextAccessor http, ClinicDbContext db)
    {
        _http = http;
        _db = db;
    }

    private ClaimsPrincipal? Principal => _http.HttpContext?.User;

    public int UserId =>
        int.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("No logged-in user.");

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    public async Task<int?> GetPatientIdAsync()
    {
        var userId = UserId;
        return await _db.Patients
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .Select(p => (int?)p.PatientId)
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetDoctorIdAsync()
    {
        var userId = UserId;
        return await _db.Doctors
            .Where(d => d.UserId == userId && d.IsActive)
            .Select(d => (int?)d.DoctorId)
            .FirstOrDefaultAsync();
    }
}
