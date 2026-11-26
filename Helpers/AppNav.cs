using System.Security.Claims;
using ClinicSystem.Models.Entities;

namespace ClinicSystem.Helpers;

/// <summary>
/// The sidebar menu for each role, in one place. Views/Shared/_Nav.cshtml renders it.
/// To add a page to a menu: add a NavItem to that role's list below.
///   ActiveFor = the actions of that controller that should highlight the item
///   (null = every action of the controller, e.g. all Drugs pages).
/// Icons are Bootstrap Icons names (https://icons.getbootstrap.com).
/// </summary>
public static class AppNav
{
    public record NavItem(string Label, string Icon, string Controller, string Action,
                          string[]? ActiveFor = null, string? CountKey = null);

    public static string RoleOf(ClaimsPrincipal user) =>
        user.IsInRole(Roles.Admin) ? Roles.Admin
        : user.IsInRole(Roles.Staff) ? Roles.Staff
        : user.IsInRole(Roles.Doctor) ? Roles.Doctor
        : user.IsInRole(Roles.Patient) ? Roles.Patient
        : "";

    /// <summary>Each role's dashboard - the landing page after login and the "home" link.</summary>
    public static (string Controller, string Action) Home(ClaimsPrincipal user) => RoleOf(user) switch
    {
        Roles.Patient => ("PatientAcc", "Index"),
        Roles.Doctor => ("DoctorAcc", "Index"),
        Roles.Staff => ("StaffAcc", "Index"),
        Roles.Admin => ("AdminAcc", "Index"),
        _ => ("Account", "Login")
    };

    public static IReadOnlyList<NavItem> For(ClaimsPrincipal user) => RoleOf(user) switch
    {
        Roles.Patient => new NavItem[]
        {
            new("Home", "bi-house", "PatientAcc", "Index"),
            new("Book appointment", "bi-calendar-plus", "Appointments", "Book", new[] { "Book" }),
            new("My appointments", "bi-calendar-check", "Appointments", "My", new[] { "My" }),
            new("Find a doctor", "bi-search", "DoctorRecommendation", "Index"),
            new("My reports", "bi-file-earmark-text", "Reports", "Index"),
        },
        Roles.Doctor => new NavItem[]
        {
            new("Today", "bi-house", "DoctorAcc", "Index"),
            new("Appointments", "bi-list-check", "Appointments", "Doctor", new[] { "Doctor" }),
            new("Schedule & leave", "bi-calendar-week", "DoctorSchedules", "Mine", new[] { "Mine" }),
            new("Drugs", "bi-capsule", "Drugs", "Index"),
            new("Reports", "bi-file-earmark-text", "Reports", "Index"),
        },
        Roles.Staff => new NavItem[]
        {
            new("Front desk", "bi-house", "StaffAcc", "Index"),
            new("Appointments", "bi-list-check", "Appointments", "Manage", new[] { "Manage" }),
            new("Doctor schedules", "bi-calendar-week", "DoctorSchedules", "Index", new[] { "Index", "Doctor" }),
            new("Conflicts", "bi-exclamation-triangle", "DoctorSchedules", "Conflicts", new[] { "Conflicts" }),
        },
        Roles.Admin => new NavItem[]
        {
            new("Overview", "bi-speedometer2", "AdminAcc", "Index"),
            new("Appointments", "bi-list-check", "Appointments", "Manage", new[] { "Manage" }),
            new("Doctor schedules", "bi-calendar-week", "DoctorSchedules", "Index", new[] { "Index", "Edit", "Doctor" }),
            new("Leave requests", "bi-airplane", "DoctorSchedules", "Leaves", new[] { "Leaves" }, CountKey: "pendingLeave"),
            new("Conflicts", "bi-exclamation-triangle", "DoctorSchedules", "Conflicts", new[] { "Conflicts" }),
            new("Drugs", "bi-capsule", "Drugs", "Index"),
            new("Reports", "bi-file-earmark-text", "Reports", "Index"),
        },
        _ => Array.Empty<NavItem>()
    };

    public static bool IsActive(NavItem item, string? controller, string? action) =>
        string.Equals(item.Controller, controller, StringComparison.OrdinalIgnoreCase)
        && (item.ActiveFor == null || item.ActiveFor.Contains(action ?? "", StringComparer.OrdinalIgnoreCase));

    /// <summary>"Patient", "Doctor", "Front desk", "Administrator" - shown under the user's name.</summary>
    public static string RoleLabel(string role) => role switch
    {
        Roles.Staff => "Front desk staff",
        Roles.Admin => "Administrator",
        _ => role
    };
}
