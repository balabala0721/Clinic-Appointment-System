using ClinicSystem.Services;
using ClinicSystem.Data;
using ClinicSystem.Models.Entities;
using ClinicSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace ClinicSystem.Controllers;

public class ReportsController : Controller

{
    private readonly DrugInventoryPdfService pdfService;
    private ClinicDbContext db;
    public ReportsController(ClinicDbContext context, DrugInventoryPdfService pdfService)
    {
        db = context;
        this.pdfService = pdfService;
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public IActionResult DownloadDrugInventoryPdf()
    {
        var drugs = db.DrugInventoryReports
            .AsNoTracking()
            .OrderBy(d => d.DrugId)
            .ToList();

        var pdf = pdfService.GeneratePdf(drugs);

        return File(
            pdf,
            "application/pdf",
            $"DrugInventoryReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
        );
    }

    private List<ReportType> GetAllowedReportTypes()
    {
        var allowedTypes = new List<ReportType>();

        if (User.IsInRole("Admin"))
        {
            allowedTypes.AddRange(new[]
            {
            ReportType.AppointmentReport,
            ReportType.PatientMedicalHistoryReport,
            ReportType.PrescriptionReport,
            ReportType.DrugInventoryReport,
            ReportType.ClinicOverallPerformanceReport
        });
        }

        if (User.IsInRole("Doctor"))
        {
            allowedTypes.AddRange(new[]
            {
            ReportType.AppointmentReport,
            ReportType.PatientMedicalHistoryReport,
            ReportType.PrescriptionReport
        });
        }

        if (User.IsInRole("Patient"))
        {
            allowedTypes.AddRange(new[]
            {
            ReportType.MedicalVisitReport,
            ReportType.AppointmentReport,
            ReportType.PrescriptionReport
        });
        }

        return allowedTypes.Distinct().ToList();
    }

    private void LoadReportTypes()
    {
        ViewBag.ReportTypes = GetAllowedReportTypes()
            .Select(type => new SelectListItem
            {
                Value = type.ToString(),
                Text = type.ToString()
            })
            .ToList();
    }

    public IActionResult Index()
    {
        var model = new ReportViewModel()
        {
            TotalPatients = db.Patients.Count(),

            TotalDoctors = db.Doctors.Count(),

            TotalAppointments = db.Appointments.Count(),

            TotalReports = db.GeneratedReportHistories.Count()
        };
        return View(model);
    }


    [Authorize]
    [HttpGet]
    public IActionResult Generate()
    {
        LoadReportTypes();

        return View(new GenerateReportViewModel());
    }


    [Authorize(Roles = Roles.Admin + "," + Roles.Doctor + "," + Roles.Patient)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Generate(GenerateReportViewModel model)
    {
        var allowedTypes = GetAllowedReportTypes();

        if (!Enum.TryParse<ReportType>(
            model.ReportType, true, out var reportType)
            || !Enum.IsDefined(typeof(ReportType), reportType)
            || !allowedTypes.Contains(reportType))
        {
            ModelState.AddModelError(
                nameof(model.ReportType),
                "You are not authorized to generate this report."
            );
        }

        if (!ModelState.IsValid)
        {
            LoadReportTypes();
            return View(model);
        }

        // Get current logged-in UserId
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized();
        }

        // Save report generation history
        var history = new GeneratedReportHistory
        {
            UserId = userId,
            ReportType = reportType,
            GeneratedDateTime = DateTime.Now
        };

        db.GeneratedReportHistories.Add(history);
        db.SaveChanges();

        return RedirectToAction(nameof(History));
    }

    public IActionResult Download()
    {

        return View();
    }


    [Authorize(Roles = Roles.Admin + "," + Roles.Doctor + "," + Roles.Patient)]
    public IActionResult History()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized();
        }

        var reports = db.GeneratedReportHistories
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.GeneratedDateTime)
            .ToList();

        return View(reports);
    }


    // Appointment Report
    [Authorize(Roles = Roles.Admin + "," + Roles.Doctor + "," + Roles.Patient)]
    [HttpGet]
    public IActionResult Appointment()
    {
        var reports = db.AppointmentReports
            .AsNoTracking();

        // Admin and Doctor can view all appointment records
        if (User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Doctor))
        {
            return View(reports
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToList());
        }

        // Patient can only view their own appointment records
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized();
        }

        var patient = db.Patients
            .AsNoTracking()
            .FirstOrDefault(p => p.UserId == userId);

        if (patient == null)
        {
            return NotFound("Patient record not found.");
        }

        var patientReports = reports
            .Where(a => a.PatientId == patient.PatientId)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToList();

        return View(patientReports);
    }

    // Consultation Report
    public IActionResult Consultation()
    {
        return View();
    }

    // Medical Visit Report
    public IActionResult MedicalVisit()
    {
        return View();
    }

    // Prescription Report
    public IActionResult Prescription()
    {
        return View();
    }

    // Patient Medical History Report
    public IActionResult PatientMedicalHistory()
    {
        return View();
    }

    // Drug Inventory Report
    public IActionResult DrugInventory()
    {
        var drugs = db.DrugInventoryReports
            .AsNoTracking()
            .ToList();

        return View(drugs);
    }

    // Clinic Overall Performance Report
    public IActionResult ClinicOverallPerformance()
    {
        return View();
    }


    [Authorize]
    [HttpGet]
    public IActionResult Details(int id)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized();
        }

        var report = db.GeneratedReportHistories
            .AsNoTracking()
            .FirstOrDefault(r => r.ReportId == id && r.UserId == userId);

        if (report == null)
        {
            return NotFound();
        }

        return View(report);
    }


}