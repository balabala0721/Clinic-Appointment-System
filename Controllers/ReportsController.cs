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

    private ClinicDbContext db;
    public ReportsController(ClinicDbContext context)
    {
        db = context;
    }

    private List<ReportType> GetAllowedReportTypes()
    {
        var allowedTypes = new List<ReportType>();

        if (User.IsInRole("Admin"))
        {
            allowedTypes.AddRange(new[]
            {
            ReportType.AppointmentSummaryReport,
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
            ReportType.AppointmentSummaryReport,
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

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Generate(GenerateReportViewModel model)
    {
        var allowedTypes = GetAllowedReportTypes();

        // Check whether selected report type is valid
        if (!Enum.TryParse<ReportType>(
            model.ReportType, true, out var reportType)
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

        // Continue with report generation and saving history here.

        return RedirectToAction(nameof(History));
    }
    public IActionResult Download()
    {

        return View();
    }

    public IActionResult History()
    {
        var reports = db.GeneratedReportHistories
            .AsNoTracking()
            .OrderByDescending(r => r.GeneratedDateTime)
            .ToList();

        return View(reports);
    }

    // Appointment Report
    [Authorize(Roles = "Patient")]
    public IActionResult Appointment()
    {
        // Get current logged-in UserID
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized();
        }

        // Find current Patient
        var patient = db.Patients
            .AsNoTracking()
            .FirstOrDefault(p => p.UserId == userId);

        if (patient == null)
        {
            return NotFound("Patient record not found.");
        }

        // Get only this patient's appointments
        var reports = db.AppointmentReports
            .AsNoTracking()
            .Where(a => a.PatientId == patient.PatientId)
            .ToList();

        return View(reports);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Details(int id)
    {
        var report = db.GeneratedReportHistories
            .AsNoTracking()
            .FirstOrDefault(r => r.ReportId == id);

        if (report == null)
        {
            return NotFound();
        }

        return View(report);
    }

    public IActionResult Customize()
    {

        return View();
    }

}
