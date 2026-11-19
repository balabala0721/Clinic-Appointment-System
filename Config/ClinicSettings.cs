namespace ClinicSystem.Config;

/// <summary>
/// Booking and scheduling rules. Change values here - nothing else in the
/// code hard-codes these numbers.
/// </summary>
public static class ClinicSettings
{
    // All appointment dates/times are stored as clinic-local time.
    // .NET 8 accepts IANA zone ids on Windows too.
    public const string ClinicTimeZoneId = "Asia/Kuala_Lumpur";

    // Booking window
    public const int MinBookingLeadMinutes = 60;    // can't book a slot starting within the next hour
    public const int MaxBookingDaysAhead = 30;      // how far ahead patients can book
    public const int MinCancelHoursBefore = 2;      // patients can't cancel/reschedule closer than this
    public const int MaxActiveAppointmentsPerPatient = 3;

    // New online bookings go straight to Confirmed. Set false if staff must confirm.
    public const bool AutoConfirmBookings = true;

    // Default slot length when admin creates a new schedule
    public const int DefaultSlotDurationMinutes = 15;

    // Doctor recommendation
    public const int RecommendationSearchDays = 14;  // look this far ahead for earliest slots
    public const int RecommendedDoctorsToShow = 3;
    public const int SlotsShownPerDoctor = 3;
    public const string FallbackSpecialisation = "General Practice";

    // Admin lists
    public const int AdminPageSize = 25;
}

/// <summary>
/// Clinic details the support chat answers questions from. PLACEHOLDERS - replace with
/// your demo clinic's real details. The AI is told to answer ONLY from these, so it
/// can't invent opening hours or prices.
/// </summary>
public static class ClinicInfo
{
    public const string Name = "TAR Health Clinic";
    public const string Address = "Jalan Genting Kelang, Setapak, 53300 Kuala Lumpur";
    public const string Phone = "03-1234 5678";
    public const string WhatsApp = "012-345 6789";
    public const string Email = "support@clinic.test";
    public const string OpeningHours = "Mon-Fri 9:00-21:00, Sat 9:00-12:00, closed Sundays and public holidays";
    public const string PaymentMethods = "cash, debit/credit card, FPX online banking, e-wallets (Touch 'n Go, GrabPay) and company panel";
    public const string WalkInPolicy = "Walk-ins are welcome, but patients with appointments are seen first";
    public const string LateArrivalPolicy = "Please arrive 10 minutes early. If you are more than 15 minutes late, your slot may be given to a walk-in patient";
    public const string Parking = "Free parking in front of the clinic";
}

/// <summary>
/// AI limits. The API key and model name live in appsettings.json / user-secrets
/// (section "Gemini") because they change per environment and the key is secret.
/// </summary>
public static class AiSettings
{
    public const int RequestTimeoutSeconds = 20;
    public const double Temperature = 0.2;           // low = more consistent, less creative
    public const int MaxOutputTokens = 2048;         // newer Gemini models "think" first, which uses tokens too

    public const int MaxProposalsPerReply = 3;       // confirm buttons shown under one assistant reply
    public const int DefaultSlotSearchDays = 7;      // when the patient doesn't give an end date
    public const int AfternoonStartsHour = 12;       // "morning" = before this hour
    public const int EveningStartsHour = 17;         // "afternoon" = before this hour

    public const int MaxUserMessageLength = 500;     // characters per chat message
    public const int MaxHistoryMessages = 10;        // earlier chat turns sent back to the model
    public const int MaxToolRoundTrips = 5;          // stops a model stuck calling tools forever
    public const int MaxSlotsReturnedToModel = 8;

    public const int MaxSymptomLength = 500;

    public const int MaxBillsShownInChat = 5;        // most recent invoices the support chat can list
}
