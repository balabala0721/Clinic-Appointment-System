using System.Security.Claims;
using System.Threading.RateLimiting;
using ClinicSystem.Config;
using ClinicSystem.Data;
using ClinicSystem.Services;
using ClinicSystem.Services.Ai;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ---- Database (one shared DbContext for the whole team)
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClinicDb")));

// ---- Auth: placeholder until the Auth module is merged.
// Login must sign in with ClaimTypes.NameIdentifier = UserId and ClaimTypes.Role = Role.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccessDenied";
    });

// ---- Appointment + scheduling services (Scoped = one instance per HTTP request)
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<SlotService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<ScheduleService>();

// ---- AI services
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient<GeminiClient>(c => c.Timeout = TimeSpan.FromSeconds(AiSettings.RequestTimeoutSeconds));
builder.Services.AddScoped<SupportChatService>();
builder.Services.AddScoped<DoctorRecommendationService>();

// ---- Rate limit for AI endpoints: each logged-in user gets N requests per minute.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(Program.AiRateLimitPolicy, http =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? http.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Program.AiRequestsPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllerRoute(name: "default", pattern: "{controller=Account}/{action=Login}/{id?}");

// Development only: apply any pending migrations (creates ClinicDb on first run), then add demo data.
// In production, run Update-Database (or a migration script) deliberately instead.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
    try
    {
        if (!db.Database.GetMigrations().Any())
            throw new InvalidOperationException(
                "No migrations found. In Package Manager Console run: Add-Migration InitialCreate");
        await db.Database.MigrateAsync();
        await DevDataSeeder.SeedAsync(db);      // TEMPORARY - remove with the test login
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database migration/seeding failed. Did you run Add-Migration InitialCreate? " +
                                "Is the ClinicDb connection string pointing at a SQL Server you can reach?");
    }
}

app.Run();

public partial class Program
{
    public const string AiRateLimitPolicy = "ai";
    public const int AiRequestsPerMinute = 10;
}
