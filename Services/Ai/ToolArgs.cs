using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ClinicSystem.Services.Ai;

/// <summary>
/// Safe readers for the arguments the model passes to a function.
/// Never trust them: the model may send "3", 3, 3.0 or nothing at all.
/// </summary>
public static class ToolArgs
{
    public static string? Str(JsonObject args, string key)
    {
        if (args[key] is not JsonValue v) return null;
        var s = v.GetValueKind() == JsonValueKind.String ? v.GetValue<string>() : v.ToJsonString();
        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    public static int? Int(JsonObject args, string key)
    {
        if (args[key] is not JsonValue v) return null;
        if (v.GetValueKind() == JsonValueKind.Number && v.TryGetValue<double>(out var d)) return (int)d;
        return int.TryParse(Str(args, key), out var i) ? i : null;
    }

    public static DateOnly? Date(JsonObject args, string key) =>
        DateOnly.TryParseExact(Str(args, key), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d : null;

    public static TimeOnly? Time(JsonObject args, string key) =>
        TimeOnly.TryParseExact(Str(args, key), new[] { "HH:mm", "H:mm", "HH:mm:ss" },
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var t)
            ? t : null;
}
