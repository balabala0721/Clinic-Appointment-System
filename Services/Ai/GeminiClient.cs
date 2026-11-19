using System.Text;
using System.Text.Json.Nodes;
using ClinicSystem.Config;
using Microsoft.Extensions.Options;

namespace ClinicSystem.Services.Ai;

/// <summary>Bound from the "Gemini" section of appsettings.json / user-secrets.</summary>
public class GeminiOptions
{
    public string ApiKey { get; set; } = "";
    // Model is now a constant (AiSettings.PrimaryModel); kept here so appsettings.json binds cleanly.
    public string Model { get; set; } = "";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
}

/// <summary>Thrown when the AI can't be reached; controllers show a friendly fallback.</summary>
public class AiUnavailableException : Exception
{
    public AiUnavailableException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>
/// Thin wrapper over the Gemini REST "generateContent" endpoint.
///
/// Retry / fallback policy (see AiSettings for the knobs):
///   - Try PrimaryModel first. On 503 or timeout, wait RetryDelayMilliseconds and retry up to
///     MaxRetriesPerModel times.
///   - On 429 (quota), skip remaining Primary retries and go straight to FallbackModel.
///   - If Primary still fails after all retries, try FallbackModel with the same rule.
///   - 400 / 401 / 403 / 404 are config / programmer errors — throw immediately (no retry).
///   - If everything fails, throw AiUnavailableException.
///
/// Thought-signature safety:
///   When switching to FallbackModel after Primary failed, StripAllThoughts() is called on a
///   copy of the request body so Primary's thought tokens never reach FallbackModel.
///   SupportChatService must also call StripThoughtsFromContents() on its in-memory history
///   after detecting a switch (see that class for details).
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

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    /// <summary>
    /// Sends a request body and returns the parsed JSON response.
    /// Tries PrimaryModel first, falls back to FallbackModel on transient errors.
    /// </summary>
    public async Task<JsonObject> GenerateAsync(JsonObject body, CancellationToken ct = default)
        => (await GenerateWithModelAsync(body, null, ct)).Response;

    /// <summary>
    /// Like GenerateAsync but also returns which model answered.
    /// Pass preferModel to start directly from a specific model — useful for locking the
    /// whole SupportChatService tool-calling loop to the same model once a fallback occurred.
    /// When preferModel == FallbackModel, Primary is skipped entirely.
    /// </summary>
    public async Task<(JsonObject Response, string ModelUsed)> GenerateWithModelAsync(
        JsonObject body, string? preferModel, CancellationToken ct = default)
    {
        if (!IsConfigured) throw new AiUnavailableException("Gemini API key is not configured.");

        var startModel = preferModel ?? AiSettings.PrimaryModel;
        // If already locked to FallbackModel, skip Primary to avoid cross-model thought errors.
        var sequence = startModel == AiSettings.FallbackModel
            ? new[] { AiSettings.FallbackModel }
            : new[] { startModel, AiSettings.FallbackModel };

        Exception? lastEx = null;

        for (var mi = 0; mi < sequence.Length; mi++)
        {
            var model = sequence[mi];

            // When stepping from Primary to Fallback, strip Primary's thought signatures from
            // a copy of the body. This copy is used only for this model's attempts.
            // (If we started directly on FallbackModel, mi is always 0, so we never strip.)
            var bodyToSend = mi > 0 ? StripAllThoughts(body) : body;

            var quota = false;
            for (var attempt = 0; attempt <= AiSettings.MaxRetriesPerModel && !quota; attempt++)
            {
                if (attempt > 0)
                    await Task.Delay(AiSettings.RetryDelayMilliseconds, ct);
                try
                {
                    var result = await SendOnceAsync(model, bodyToSend, ct);
                    _logger.LogInformation("Gemini answered using {Model} (attempt {N})", model, attempt + 1);
                    return (result, model);
                }
                catch (AiQuotaException)
                {
                    _logger.LogWarning("Gemini {Model}: 429 quota – going to fallback", model);
                    lastEx = new AiUnavailableException($"Quota exceeded for model {model}.");
                    quota = true; // break inner loop, proceed to FallbackModel
                }
                catch (AiRetryableException ex)
                {
                    _logger.LogWarning("Gemini {Model} attempt {N}/{Max}: {Status}",
                        model, attempt + 1, AiSettings.MaxRetriesPerModel + 1, ex.StatusCode);
                    lastEx = ex;
                }
                // AiUnavailableException (400/401/403/404) propagates without catching
            }
        }

        throw new AiUnavailableException("AI service unavailable after all retries.", lastEx);
    }

    private async Task<JsonObject> SendOnceAsync(string model, JsonObject body, CancellationToken ct)
    {
        var url = $"{_options.BaseUrl.TrimEnd('/')}/models/{model}:generateContent";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-goog-api-key", _options.ApiKey);
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Gemini {Model}: network error or timeout", model);
            throw new AiRetryableException($"Network error calling {model}.", 503, ex);
        }

        var text = await response.Content.ReadAsStringAsync(ct);
        var status = (int)response.StatusCode;

        if (response.IsSuccessStatusCode)
            return JsonNode.Parse(text) as JsonObject
                   ?? throw new AiUnavailableException("AI service returned an empty response.");

        switch (status)
        {
            case 429:
                throw new AiQuotaException(model);
            case 503:
                _logger.LogWarning("Gemini {Model} 503 (overloaded): {Snippet}",
                    model, text.Length > 300 ? text[..300] : text);
                throw new AiRetryableException($"Model {model} overloaded.", 503);
            case 400 or 401 or 403 or 404:
                _logger.LogError("Gemini {Model} {Status} – config/auth error: {Snippet}",
                    model, status, text.Length > 500 ? text[..500] : text);
                throw new AiUnavailableException($"Gemini config error: HTTP {status}.");
            default:
                _logger.LogWarning("Gemini {Model} {Status}: {Snippet}",
                    model, status, text.Length > 300 ? text[..300] : text);
                throw new AiRetryableException($"Gemini {model} returned {status}.", status);
        }
    }

    // ---- thought-signature helpers ----------------------------------------

    /// <summary>
    /// Returns a deep clone of body with all thought parts stripped from every content entry.
    /// Used when switching to a different model to prevent cross-model thought-signature errors.
    /// </summary>
    private static JsonObject StripAllThoughts(JsonObject body)
    {
        var clone = (JsonNode.Parse(body.ToJsonString()) as JsonObject)!;
        if (clone["contents"] is JsonArray contents)
            StripThoughtsFromContents(contents);
        return clone;
    }

    /// <summary>
    /// Removes thought parts in-place from a contents array.
    /// SupportChatService calls this on its own contents reference after detecting a model switch
    /// so that future rounds in the same ChatAsync call don't send stale thought signatures.
    /// </summary>
    public static void StripThoughtsFromContents(JsonArray contents)
    {
        foreach (var item in contents.OfType<JsonObject>())
        {
            if (item["parts"] is not JsonArray parts) continue;
            for (var i = parts.Count - 1; i >= 0; i--)
            {
                if (parts[i] is JsonObject part && part["thought"]?.GetValue<bool>() == true)
                    parts.RemoveAt(i);
            }
        }
    }

    // ---- JSON helpers (unchanged) -----------------------------------------

    public static JsonObject TextContent(string role, string text) => new()
    {
        ["role"] = role,
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

// File-scoped: internal retry-control signals. Never escape GeminiClient.
file sealed class AiRetryableException : Exception
{
    public int StatusCode { get; }
    public AiRetryableException(string msg, int statusCode, Exception? inner = null)
        : base(msg, inner) { StatusCode = statusCode; }
}

file sealed class AiQuotaException : Exception
{
    public AiQuotaException(string model) : base($"Quota exceeded for {model}.") { }
}
