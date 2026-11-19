using ClinicSystem.Models.Entities;

namespace ClinicSystem.Services;

/// <summary>
/// Pure functions (no database) that decide whether a doctor works at a given
/// time. Shared by SlotService (generating slots) and ScheduleService
/// (finding appointments that clash after a timetable change), so both always
/// agree on the rules.
/// </summary>
public static class ScheduleRules
{
    public static bool Overlaps(TimeOnly aStart, TimeOnly aEnd, TimeOnly bStart, TimeOnly bEnd) =>
        aStart < bEnd && bStart < aEnd;

    /// <summary>The schedule row that applies on this date (newest EffectiveFrom wins).</summary>
    public static DoctorSchedule? ScheduleFor(IEnumerable<DoctorSchedule> schedules, DateOnly date) =>
        schedules.Where(s => s.AppliesOn(date))
                 .OrderByDescending(s => s.EffectiveFrom)
                 .FirstOrDefault();

    public static bool IsOnLeave(IEnumerable<DoctorLeave> approvedLeaves, DateOnly date, TimeOnly start, TimeOnly end) =>
        approvedLeaves.Any(l =>
            l.StartDate <= date && l.EndDate >= date &&
            (l.IsWholeDay || Overlaps(start, end, l.StartTime!.Value, l.EndTime!.Value)));

    public static bool IsInBreak(DoctorSchedule s, TimeOnly start, TimeOnly end) =>
        s.BreakStart.HasValue && s.BreakEnd.HasValue &&
        Overlaps(start, end, s.BreakStart.Value, s.BreakEnd.Value);

    /// <summary>True if the doctor is scheduled to work for the whole of start..end on this date.</summary>
    public static bool IsWorkingTime(
        IEnumerable<DoctorSchedule> schedules, IEnumerable<DoctorLeave> approvedLeaves,
        DateOnly date, TimeOnly start, TimeOnly end)
    {
        var schedule = ScheduleFor(schedules, date);
        if (schedule == null) return false;
        if (start < schedule.StartTime || end > schedule.EndTime) return false;
        if (IsInBreak(schedule, start, end)) return false;
        return !IsOnLeave(approvedLeaves, date, start, end);
    }
}
