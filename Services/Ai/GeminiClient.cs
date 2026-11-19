using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace ClinicSystem.Services.Ai;

/// <summary>Bound from the "Gemini" section of appsettings.json / user-secrets.</summary>
public class GeminiOptions
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
}

/// <summary>Thrown when the AI can't be reached; controllers show a friendly fallback.</summary>
public class AiUnavailableException : Exception
{
    public AiUnavailableException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>
/// Thin wrapper over the Gemini REST "generateContent" endpoint. Both AI features
/// use this one class, so the HTTP/auth/error handling is written once.
///
/// We use plain HttpClient + JSON instead of an SDK so you can see exactly what is
/// sent - and it means no extra NuGet package.
/// </summary>
public class GeminiClient
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiClient> _logger;

    public GeminiClient(HttpClient http, IOptions<GeminiOptions> options, ILogger<GeminiClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey) && !string.IsNullOrWhiteSpace(_options.Model);

    /// <summary>Sends a request body and returns the parsed JSON response.</summary>
    public async Task<JsonObject> GenerateAsync(JsonObject body, CancellationToken ct = default)
    {
        if (!IsConfigured) throw new AiUnavailableException("Gemini API key or model is not configured.");

        var url = $"{_options.BaseUrl.TrimEnd('/')}/models/{_options.Model}:generateContent";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        // Key goes in a header, not the URL, so it doesn't end up in server logs.
        request.Headers.Add("x-goog-api-key", _options.ApiKey);
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Gemini request failed or timed out");
            throw new AiUnavailableException("AI service could not be reached.", ex);
        }

        var text = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            // 429 = free-tier rate limit hit; 400 usually = wrong model name or bad request JSON.
            _logger.LogWarning("Gemini returned {Status}: {Body}", (int)response.StatusCode,
                text.Length > 500 ? text[..500] : text);
            throw new AiUnavailableException($"AI service returned {(int)response.StatusCode}.");
        }

        return JsonNode.Parse(text) as JsonObject
               ?? throw new AiUnavailableException("AI service returned an empty response.");
    }

    // ------------------------------------------------------------ JSON helpers

    public static JsonObject TextContent(string role, string text) => new()
    {
        ["role"] = role,                                    // "user" or "model"
        ["parts"] = new JsonArray(new JsonObject { ["text"] = text })
    };

    public static JsonObject SystemInstruction(string text) => new()
    {
        ["parts"] = new JsonArray(new JsonObject { ["text"] = text })
    };

    /// <summary>The model's reply content (role + parts) from the first candidate.</summary>
    public static JsonObject? FirstContent(JsonObject response) =>
        response["candidates"]?[0]?["content"] as JsonObject;

    /// <summary>Joins all text parts, skipping the model's internal "thought" parts.</summary>
    public static string GetText(JsonObject? content)
    {
        if (content?["parts"] is not JsonArray parts) return "";
        var sb = new StringBuilder();
        foreach (var part in parts.OfType<JsonObject>())
        {
            if (part["thought"]?.GetValue<bool>() == true) continue;
            if (part["text"] is JsonValue t) sb.Append(t.GetValue<string>());
        }
        return sb.ToString().Trim();
    }

    public record FunctionCall(string Name, JsonObject Args, string? Id);

    public static List<FunctionCall> GetFunctionCalls(JsonObject? content)
    {
        var calls = new List<FunctionCall>();
        if (content?["parts"] is not JsonArray parts) return calls;
        foreach (var part in parts.OfType<JsonObject>())
        {
            if (part["functionCall"] is not JsonObject fc) continue;
            var name = fc["name"]?.GetValue<string>() ?? "";
            var args = fc["args"] as JsonObject ?? new JsonObject();
            calls.Add(new FunctionCall(name, args, fc["id"]?.GetValue<string>()));
        }
        return calls;
    }

    public static JsonObject FunctionResponsePart(FunctionCall call, JsonObject result)
    {
        var fr = new JsonObject { ["name"] = call.Name, ["response"] = result };
        if (call.Id != null) fr["id"] = call.Id;
        return new JsonObject { ["functionResponse"] = fr };
    }
}
