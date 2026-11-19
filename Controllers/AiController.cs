using ClinicSystem.Config;
using ClinicSystem.Models.Entities;
using ClinicSystem.Models.ViewModels;
using ClinicSystem.Services;
using ClinicSystem.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicSystem.Controllers;

/// <summary>AI Support Chat endpoint - the floating chat widget (Appointment Management module).</summary>
[Authorize(Roles = Roles.Patient)]
[EnableRateLimiting(Program.AiRateLimitPolicy)]     // stops one user burning the API quota
public class SupportChatController : Controller
{
    private readonly SupportChatService _chat;
    private readonly CurrentUserService _me;
    private readonly ILogger<SupportChatController> _logger;

    public SupportChatController(SupportChatService chat, CurrentUserService me, ILogger<SupportChatController> logger)
    {
        _chat = chat;
        _me = me;
        _logger = logger;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send([FromBody] AssistantChatRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return Json(new { success = false, reply = "Please type a shorter message." });

        var patientId = await _me.GetPatientIdAsync();
        if (patientId == null) return Forbid();

        try
        {
            var result = await _chat.ChatAsync(patientId.Value, request.History, request.Message, ct);
            return Json(new
            {
                success = true,
                reply = result.Reply,
                proposals = result.Proposals,
                links = result.Links,
                isEmergency = result.IsEmergency,
                disclaimer = result.Disclaimer
            });
        }
        catch (AiUnavailableException ex)
        {
            _logger.LogWarning(ex, "Support chat unavailable");
            return Json(new
            {
                success = false,
                reply = "The assistant is busy right now. Please try again in a minute, or use the Book Appointment page."
            });
        }
    }
}

/// <summary>AI Doctor Recommendation (Doctor Schedule Management module).</summary>
[Authorize]
public class DoctorRecommendationController : Controller
{
    private readonly DoctorRecommendationService _recommender;
    private readonly ILogger<DoctorRecommendationController> _logger;

    public DoctorRecommendationController(DoctorRecommendationService recommender,
        ILogger<DoctorRecommendationController> logger)
    {
        _recommender = recommender;
        _logger = logger;
    }

    public IActionResult Index() => View();

    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting(Program.AiRateLimitPolicy)]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request, CancellationToken ct)
    {
        // Emergency check runs even when the request is invalid or the AI is down.
        if (DoctorRecommendationService.LooksLikeEmergency(request.Symptoms ?? ""))
            return Json(new { success = true, isEmergency = true, message = DoctorRecommendationService.EmergencyMessage });

        if (!ModelState.IsValid) return Json(new { success = false, message = "Please describe your symptoms (up to 500 characters)." });

        try
        {
            var result = await _recommender.RecommendAsync(request.Symptoms ?? "", ct);
            if (!result.Success) return Json(new { success = false, message = result.Message });

            var r = result.Value!;
            return Json(new
            {
                success = true,
                isEmergency = r.IsEmergency,
                message = r.Message,
                specialisation = r.Specialisation,
                reason = r.Reason,
                urgency = r.Urgency,
                doctors = r.Doctors.Select(d => new
                {
                    d.DoctorId,
                    d.Name,
                    fee = d.ConsultationFee.ToString("0.00"),
                    slots = d.Slots.Select(s => new
                    {
                        date = s.Date.ToString("yyyy-MM-dd"),
                        dateLabel = s.Date.ToString("ddd d MMM"),
                        time = s.Start.ToString("HH:mm")
                    })
                })
            });
        }
        catch (AiUnavailableException ex)
        {
            _logger.LogWarning(ex, "Doctor recommendation unavailable");
            return Json(new { success = false, message = "Recommendations are unavailable right now. You can still browse doctors and book directly." });
        }
    }
}
