namespace ClinicSystem.Config;

/// <summary>
/// "Now" in clinic-local time. Server clocks (Azure, etc.) usually run in UTC,
/// so DateTime.Now would be 8 hours off for a Malaysian clinic.
/// </summary>
public static class ClinicClock
{
    private static readonly TimeZoneInfo ClinicZone =
        TimeZoneInfo.FindSystemTimeZoneById(ClinicSettings.ClinicTimeZoneId);

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ClinicZone);
    public static DateOnly Today => DateOnly.FromDateTime(Now);
    public static TimeOnly TimeNow => TimeOnly.FromDateTime(Now);

    /// <summary>Converts a UTC timestamp from the database (e.g. CreatedAt) to clinic-local time.</summary>
    public static DateTime FromUtc(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), ClinicZone);
}
