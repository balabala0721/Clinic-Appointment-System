
using ClinicSystem.Data;
using ClinicSystem.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Controllers;

public class DrugsController : Controller
{
    private readonly ClinicDbContext db;

    public DrugsController(ClinicDbContext context)
    {
        db = context;
    }

    // Display all drugs


    public async Task<IActionResult> Index(
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        string? medicalUses,
        string? stockAvailability,
        string? sortOrder)
    {
        var query = db.Drugs.AsQueryable();

        // Non-admin users can only view active drugs
        if (!User.IsInRole("Admin"))
        {
            query = query.Where(d => d.IsActive);
        }

        // Filter by category
        if (!string.IsNullOrEmpty(category) &&
            Enum.TryParse<DrugCategory>(category, out var selectedCategory))
        {
            query = query.Where(d => d.DrugCategory == selectedCategory);
        }

        // Filter by minimum price
        if (minPrice.HasValue)
        {
            query = query.Where(d => d.UnitPrice >= minPrice.Value);
        }

        // Filter by maximum price
        if (maxPrice.HasValue)
        {
            query = query.Where(d => d.UnitPrice <= maxPrice.Value);
        }

        // Filter by medical uses
        if (!string.IsNullOrWhiteSpace(medicalUses))
        {
            query = query.Where(d =>
                d.MedicalUses.Contains(medicalUses));
        }


        // Sorting

        switch (sortOrder)
        {
            case "name_asc":
                query = query.OrderBy(d => d.DrugName);
                break;

            case "name_desc":
                query = query.OrderByDescending(d => d.DrugName);
                break;

            case "category_asc":
                query = query.OrderBy(d => d.DrugCategory);
                break;

            case "category_desc":
                query = query.OrderByDescending(d => d.DrugCategory);
                break;

            case "price_asc":
                query = query.OrderBy(d => d.UnitPrice);
                break;

            case "price_desc":
                query = query.OrderByDescending(d => d.UnitPrice);
                break;

            case "stock_asc":
                query = query.OrderBy(d => d.StockQuantity);
                break;

            case "stock_desc":
                query = query.OrderByDescending(d => d.StockQuantity);
                break;


            case "id_asc":
                query = query.OrderBy(d => d.DrugID);
                break;

            case "id_desc":
                query = query.OrderByDescending(d => d.DrugID);
                break;

            case "status_asc":
                query = query.OrderBy(d => d.StockQuantity == 0 ? 2 :
                                           d.StockQuantity < d.MinStockLevel ? 1 : 0);
                break;

            case "status_desc":
                query = query.OrderByDescending(d => d.StockQuantity == 0 ? 2 :
                                                    d.StockQuantity < d.MinStockLevel ? 1 : 0);
                break;

            case "availability_asc":
                query = query.OrderBy(d => d.IsActive);
                break;

            case "availability_desc":
                query = query.OrderByDescending(d => d.IsActive);
                break;


            default:
                query = query.OrderBy(d => d.DrugName);
                break;
        }


        var drugs = await query.ToListAsync();


        // Filter by stock availability
        if (!string.IsNullOrEmpty(stockAvailability))
        {
            if (stockAvailability == "InStock")
            {
                drugs = drugs
                    .Where(d => d.StockStatus == StockStatus.InStock)
                    .ToList();
            }
            else if (stockAvailability == "LowStock")
            {
                drugs = drugs
                    .Where(d => d.StockStatus == StockStatus.LowStock)
                    .ToList();
            }
            else if (stockAvailability == "OutOfStock")
            {
                drugs = drugs
                    .Where(d => d.StockStatus == StockStatus.OutOfStock)
                    .ToList();
            }
        }

        ViewBag.DrugCategories = new SelectList(
            Enum.GetValues<DrugCategory>(), category);

        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.MedicalUses = medicalUses;
        ViewBag.StockAvailability = stockAvailability;
        ViewBag.SortOrder = sortOrder;

        return View(drugs);
    }



    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LowStock()
    {
        var lowStockDrugs = await db.Drugs
            .Where(d => d.IsActive &&
                        d.StockQuantity < d.MinStockLevel)
            .OrderBy(d => d.StockQuantity)
            .ToListAsync();

        return View(lowStockDrugs);
    }


    // Display drug details
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var drug = await db.Drugs
            .FirstOrDefaultAsync(d => d.DrugID == id);

        if (drug == null)
            return NotFound();

        return View(drug);
    }


    // GET: Drugs/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewBag.DrugCategories = new SelectList(
            Enum.GetValues<DrugCategory>()
        );

        return View();
    }

    // POST: Drugs/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Drug drug)
    {
        if (ModelState.IsValid)
        {
            // Generate Drug ID automatically
            var lastDrug = await db.Drugs
                .OrderByDescending(d => d.DrugID)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastDrug != null)
            {
                string lastNumber = lastDrug.DrugID.Substring(3);

                if (int.TryParse(lastNumber, out int number))
                {
                    nextNumber = number + 1;
                }
            }

            drug.DrugID = $"DRG{nextNumber:D7}";

            db.Drugs.Add(drug);

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Drug added successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Reload category dropdown if validation fails
        ViewBag.DrugCategories = new SelectList(
            Enum.GetValues<DrugCategory>()
        );

        return View(drug);
    }


    // GET: Drugs/Edit/DRG0000001
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var drug = await db.Drugs
            .FirstOrDefaultAsync(d => d.DrugID == id);

        if (drug == null)
            return NotFound();

        ViewBag.DrugCategories = new SelectList(
            Enum.GetValues<DrugCategory>(),
            drug.DrugCategory
        );

        return View(drug);
    }

    // POST: Drugs/Edit/DRG0000001
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id, Drug drug)
    {
        if (id != drug.DrugID)
            return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.DrugCategories = new SelectList(
                Enum.GetValues<DrugCategory>(),
                drug.DrugCategory
            );

            return View(drug);
        }

        var existingDrug = await db.Drugs
            .FirstOrDefaultAsync(d => d.DrugID == id);

        if (existingDrug == null)
            return NotFound();

        // Update editable fields only
        existingDrug.DrugName = drug.DrugName;
        existingDrug.DrugCategory = drug.DrugCategory;
        existingDrug.Description = drug.Description;
        existingDrug.MedicalUses = drug.MedicalUses;
        existingDrug.UnitPrice = drug.UnitPrice;
        existingDrug.StockQuantity = drug.StockQuantity;
        existingDrug.MinStockLevel = drug.MinStockLevel;

        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Drug updated successfully!";

        return RedirectToAction(nameof(Index));
    }


    // GET: Drugs/Deactivate/DRG0000001
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var drug = await db.Drugs
            .FirstOrDefaultAsync(d => d.DrugID == id);

        if (drug == null)
            return NotFound();

        return View(drug);
    }

    // POST: Drugs/Deactivate/DRG0000001
    [HttpPost, ActionName("Deactivate")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateConfirmed(string id)
    {
        var drug = await db.Drugs
            .FirstOrDefaultAsync(d => d.DrugID == id);

        if (drug == null)
            return NotFound();

        drug.IsActive = false;

        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Drug deactivated successfully!";

        return RedirectToAction(nameof(Index));
    }


    // GET: Drugs/Activate/DRG0000001
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var drug = await db.Drugs
            .FirstOrDefaultAsync(d => d.DrugID == id);

        if (drug == null)
            return NotFound();

        return View(drug);
    }

    // POST: Drugs/Activate/DRG0000001
    [HttpPost, ActionName("Activate")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateConfirmed(string id)
    {
        var drug = await db.Drugs
            .FirstOrDefaultAsync(d => d.DrugID == id);

        if (drug == null)
            return NotFound();

        drug.IsActive = true;

        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Drug activated successfully!";

        return RedirectToAction(nameof(Index));
    }


}
