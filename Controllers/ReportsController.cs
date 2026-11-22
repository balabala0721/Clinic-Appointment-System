using ClinicSystem.Data;
using ClinicSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Controllers;

public class ReportsController : Controller
{

    private ClinicDbContext db;
    public ReportsController(ClinicDbContext context)
    {
        db = context;
    }

    public IActionResult Index()
    {
        var model = new ReportViewModel();

        model.TotalPatients = db.Patients.Count();

        model.TotalDoctors = db.Doctors.Count();

        model.TotalAppointments = db.Appointments.Count();

        model.TotalReports = db.GeneratedReportHistories.Count();

        return View();
    }

    public IActionResult Generate()
    {

        return View();
    }

    public IActionResult Details()
    {

        return View();
    }

    public IActionResult Download()
    {

        return View();
    }

    public IActionResult History()
    {

        return View();
    }

    public IActionResult Delete()
    {

        return View();
    }
}
