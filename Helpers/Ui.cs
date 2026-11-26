using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicSystem.Helpers;

/// <summary>
/// Display helpers for views - one place for every date, time, money and status format,
/// so all pages look the same. UI only: nothing here touches data.
///   Dates  25/11/2026        Ui.Date(d)
///   Times  2:30 PM           Ui.Time(t)
///   Money  RM 1,250.00       Ui.Money(m)
///   Status coloured pill     @Ui.StatusBadge(a.Status)   (colours live in theme.css: .status-*)
/// The same rules exist in JS as Clinic.fmtTime / Clinic.statusBadge (wwwroot/js/clinic.js).
/// </summary>
public static class Ui
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public const string DateFormat = "dd/MM/yyyy";
    public const string TimeFormat = "h:mm tt";

    public static string Date(DateOnly d) => d.ToString(DateFormat, Invariant);
    public static string Date(DateTime d) => d.ToString(DateFormat, Invariant);
    public static string Date(DateOnly? d) => d.HasValue ? Date(d.Value) : "-";

    /// <summary>"Tue, 25/11/2026" - the weekday helps people place a date.</summary>
    public static string DayDate(DateOnly d) => d.ToString("ddd, " + DateFormat, Invariant);

    public static string Time(TimeOnly t) => t.ToString(TimeFormat, Invariant);
    public static string Time(TimeOnly? t) => t.HasValue ? Time(t.Value) : "-";
    public static string Time(DateTime t) => t.ToString(TimeFormat, Invariant);
    public static string TimeRange(TimeOnly from, TimeOnly to) => $"{Time(from)} – {Time(to)}";

    public static string DateTimeText(DateTime d) => $"{Date(d)}, {Time(d)}";

    public static string Money(decimal amount) => "RM " + amount.ToString("N2", Invariant);

    /// <summary>Weekday name for DoctorSchedule.DayOfWeek (0 = Sunday).</summary>
    public static string DayName(int dayOfWeek, bool shortName = false)
    {
        var name = ((DayOfWeek)dayOfWeek).ToString();
        return shortName ? name[..3] : name;
    }

    /// <summary>Morning / Afternoon / Evening - same split as the slot chips in clinic.js.</summary>
    public static string PartOfDay(TimeOnly t) => t.Hour < 12 ? "Morning" : t.Hour < 17 ? "Afternoon" : "Evening";

    /// <summary>Human label for a status value: CheckedIn -> "Checked in", NoShow -> "No-show".</summary>
    public static string StatusLabel(string? status) => status switch
    {
        null or "" => "-",
        "CheckedIn" => "Checked in",
        "NoShow" => "No-show",
        "PartiallyPaid" => "Partly paid",
        _ => status
    };

    /// <summary>One badge style for appointment, leave, invoice and payment statuses.</summary>
    public static IHtmlContent StatusBadge(string? status)
    {
        var span = new TagBuilder("span");
        span.AddCssClass("status-badge");
        if (!string.IsNullOrEmpty(status)) span.AddCssClass("status-" + status.ToLowerInvariant());
        span.InnerHtml.Append(StatusLabel(status));   // Append = HTML-encoded
        return span;
    }

    /// <summary>Initials for the avatar circle: "Dr Tan Wei Ming" -> "TW".</summary>
    public static string Initials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Where(p => !p.Equals("Dr", StringComparison.OrdinalIgnoreCase) && !p.Equals("Dr.", StringComparison.OrdinalIgnoreCase)
                                    && !p.Contains('@'))
                        .ToList();
        if (parts.Count == 0) return name[..1].ToUpperInvariant();
        return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
    }
}

/// <summary>Model for Views/Shared/_Pager.cshtml (Previous / Next links that keep the current filters).</summary>
public record PagerModel(int Page, bool HasNext);
