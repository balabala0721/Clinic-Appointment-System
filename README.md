# Appointment Management + Doctor Schedule Management

ASP.NET Core MVC (.NET 8) · SQL Server · EF Core · jQuery/AJAX · Gemini API

## Setup

1. **Database (EF Core migrations, code-first)**: the C# classes in `Models/Entities` + `Data/ClinicDbContext.cs`
   define the database. There is no SQL script to run.
   - First time only, in **Tools > NuGet Package Manager > Package Manager Console**:
     ```
     Add-Migration InitialCreate
     ```
     Then open the new `Migrations/..._InitialCreate.cs` and add `ReportViews.Create(migrationBuilder);` as the
     last line of `Up()` and `ReportViews.Drop(migrationBuilder);` as the first line of `Down()` (creates the report views).
   - Press F5. In Development the app runs the migrations itself (creates `ClinicDb`), then adds demo data.
     Or run `Update-Database` in the console yourself.
   - **Changing the database later**: edit the classes, then `Add-Migration DescribeChange` and `Update-Database`.
     Stop the running app first, or the build inside Add-Migration fails.
2. **Connection string**: edit `ConnectionStrings:ClinicDb` in `appsettings.json`.
3. **AI key** (free from Google AI Studio). Do NOT put it in appsettings.json, because that file goes to Git.
   In Visual Studio: right-click project → *Manage User Secrets*, then add:
   ```json
   { "Gemini": { "ApiKey": "YOUR_KEY" } }
   ```
   Check the model name in AI Studio and update `Gemini:Model` if `gemini-3.5-flash` isn't listed.
   If the key is empty, the app still runs: the assistant panel is hidden and recommendations show a fallback message.
4. **NuGet**: Visual Studio restores `Microsoft.EntityFrameworkCore.SqlServer` automatically.
5. **Login dependency**: the Auth module must sign users in with these claims:
   - `ClaimTypes.NameIdentifier` = `Users.UserId`
   - `ClaimTypes.Role` = `Patient` / `Doctor` / `Staff` / `Admin`

   `CurrentUserService` reads these claims. Until login is merged, you can't open the pages.

## Merging into the team project

- Copy `Config/`, `Services/`, `Controllers/`, the views and `wwwroot/js/*.js` + `clinic-modules.css`.
- Add the DbSets and the `OnModelCreating` lines from `Data/ClinicDbContext.cs` into the team's single DbContext.
- Copy the service registrations and the rate limiter from `Program.cs`.
- From `_Layout.cshtml`, keep: `@Html.AntiForgeryToken()`, `<partial name="_Alerts" />` and the `clinic.js` script tag.

## File map

| File | What it does |
|---|---|
| `Data/ClinicDbContext.cs` | **Database design**: tables, keys, unique indexes, CHECK rules, double-booking index |
| `Data/ReportViews.cs` | SQL views for Report Management, added in the InitialCreate migration |
| `Config/ClinicSettings.cs` | **Every adjustable value**: booking window, cancel cutoff, slot length, AI limits |
| `Config/ClinicClock.cs` | Malaysia time. The server may run in UTC, so never use `DateTime.Now` for appointments |
| `Services/ScheduleRules.cs` | Pure rules for "is the doctor working at this time?", shared by slots and conflicts |
| `Services/SlotService.cs` | Generates free slots from the timetable (slots are calculated, not stored) |
| `Services/AppointmentService.cs` | Book / reschedule / cancel / status: all booking rules live here |
| `Services/ScheduleService.cs` | Timetables, leave approval, conflict detection |
| `Services/Ai/GeminiClient.cs` | One shared wrapper for the Gemini REST API |
| `Services/Ai/BookingAssistantService.cs` | **AI feature 1**: chat booking via function calling |
| `Services/Ai/DoctorRecommendationService.cs` | **AI feature 2**: symptoms → specialisation → earliest doctor |

## How the two AI features work

**Booking assistant (Appointment module)**: Gemini *function calling*.
The model receives a list of functions (`find_available_slots`, `get_my_appointments`, `propose_booking`, ...).
When it wants data, it replies "call find_available_slots(date_from=2026-11-10)". Our C# runs that against
SQL Server and sends the result back. This loops until the model replies in text (max 5 rounds).
The model can only **propose** a change. The Confirm button calls the same `/Appointments/Book`
endpoint as the normal form, so every rule is checked again.

**Doctor recommendation (Schedule module)**: three steps, and only the middle one uses AI:
1. Emergency keyword check, in plain C# and in English + Malay. It runs *before* the AI, so it works even if the AI is down.
2. Gemini returns **structured JSON**. `responseSchema` forces it to pick one of *our* specialisations.
3. C# ranks that specialisation's doctors by earliest free slot from the schedule data.

## Safety and cost points for your report

- **Double-booking**: the filtered unique index `UX_Appointments_DoctorSlot` blocks simultaneous bookings at the database level. `TrySaveAsync` turns that error into "slot just taken".
- **Data isolation**: the AI functions get `patientId` from the login cookie, never from the model, so a prompt like "show me patient 5's bookings" can't reach other patients' data.
- **Privacy**: only doctor names, dates, times and the symptom text are sent to Google, never IC numbers or names. On Gemini's **free tier, Google may use the prompts to improve its products**. That's fine for a demo with fake data, but a real clinic would need the paid tier and PDPA consent.
- **XSS**: all AI and user text is inserted with jQuery `.text()`, never `.html()`.
- **Cost / abuse**: the rate limiter allows 10 AI requests per user per minute (`Program.AiRequestsPerMinute`). The free tier should cover an FYP demo; paid usage would likely be a few ringgit at most (estimate, check current pricing).

## Demo test checklist

- [ ] Book a slot → it disappears from the list.
- [ ] Open two browsers and book the same slot at the same time → one gets "someone just booked that slot".
- [ ] Try to cancel within 2 hours of the appointment → rejected.
- [ ] Admin adds leave over a booked appointment → it shows under **Conflicts**.
- [ ] Change a doctor's hours → the affected appointments show under Conflicts.
- [ ] Assistant: "book Dr Tan next Monday morning" → a confirm card appears, and nothing is booked until you click Confirm.
- [ ] Assistant: "I have a rash, who should I see?" → it links to Find a doctor instead of giving advice.
- [ ] Recommendation: "chest pain" / "sesak nafas" → emergency message, with no AI call.
- [ ] Recommendation: "child with fever" → Paediatrics, with the earliest slots.
- [ ] Remove the API key → the app still works without the AI features.

## Temporary test login (Development only)

- Run the app. It opens on **/Account/Login**, which lists every user. Click one to log in as them, with no password.
- **Switch user** (top right) logs you out and goes back to the list.
- **Reset demo appointments & leave** (bottom of the login page) restores the sample appointments after testing.
- All of this returns 404 outside Development. Delete `AccountController.cs`, `Views/Account/` and
  `Data/DevDataSeeder.cs`, and the seeding block in `Program.cs`, once the real Auth module is merged.

Demo data (dates are relative to the day you first run it):

| Login | Role | Use it to test |
|---|---|---|
| Ali bin Ahmad | Patient | Book, cancel/reschedule his upcoming appointment, AI assistant |
| Wong Mei Ling | Patient | Paediatrics booking, AI doctor recommendation |
| Siva Raman | Patient | Has an appointment that clashes with Dr Tan's leave |
| Dr Tan Wei Ming | Doctor | **My appointments**: yesterday's visit is waiting, mark it Completed or No-show; see reasons for visit |
| Dr Lim / Dr Rajesh / Dr Aisyah | Doctor | My schedule, request leave, see leave status (Dr Lim has one pending) |
| staff / admin | Staff, Admin | Manage appointments (status + **Reschedule**), schedules, approve Dr Lim's leave, Conflicts page |

To start completely fresh: run `Drop-Database` in Package Manager Console, then restart the app. It rebuilds everything.
