using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFornitori.Data;
using SistemaFornitori.Services;
using SistemaFornitori.ViewModels;

namespace SistemaFornitori.Controllers;

public class ImportController : Controller
{
    private readonly AppDbContext _db;
    private readonly ImportService _importService;
    private readonly MappingService _mappingService;
    private readonly InterscambioService _interscambioService;

    public ImportController(
        AppDbContext db,
        ImportService importService,
        MappingService mappingService,
        InterscambioService interscambioService)
    {
        _db = db;
        _importService = importService;
        _mappingService = mappingService;
        _interscambioService = interscambioService;
    }

    public async Task<IActionResult> Index(string? fornitore, string? brand, string? articolo, string? colore, string? taglia)
    {
        // Query base per filtrare i dropdown a cascata
        var queryFiltrata = _db.ArticoliFornitori.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(fornitore))
            queryFiltrata = queryFiltrata.Where(a => a.Fornitore == fornitore);
        if (!string.IsNullOrEmpty(brand))
            queryFiltrata = queryFiltrata.Where(a => a.Brand == brand);
        if (!string.IsNullOrEmpty(colore))
            queryFiltrata = queryFiltrata.Where(a => a.Colore == colore);
        if (!string.IsNullOrEmpty(taglia))
            queryFiltrata = queryFiltrata.Where(a => a.Taglia == taglia);

        // Query per i dropdown — ogni filtro dipende dai filtri precedenti
        var queryPerBrand = _db.ArticoliFornitori.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(fornitore))
            queryPerBrand = queryPerBrand.Where(a => a.Fornitore == fornitore);

        var queryPerColore = queryPerBrand;
        if (!string.IsNullOrEmpty(brand))
            queryPerColore = queryPerColore.Where(a => a.Brand == brand);

        var queryPerTaglia = queryPerColore;
        if (!string.IsNullOrEmpty(colore))
            queryPerTaglia = queryPerTaglia.Where(a => a.Colore == colore);

        var vm = new ImportViewModel
        {
            Fornitori = _mappingService.GetMappingsDisponibili(),
            Articoli = new List<Models.ArticoloFornitore>(),
            FiltroFornitore = fornitore,
            FiltroBrand = brand,
            FiltroArticolo = articolo,
            FiltroColore = colore,
            FiltroTaglia = taglia,

            // Fornitori — sempre tutti
            FornitoriDistinti = await _db.ArticoliFornitori
                .AsNoTracking()
                .Where(a => a.Fornitore != null && a.Fornitore != "")
                .Select(a => a.Fornitore)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(),

            // Brand — filtrati per fornitore selezionato
            BrandDistinti = await queryPerBrand
                .Where(a => a.Brand != null && a.Brand != "")
                .Select(a => a.Brand)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(),

            // Colori — filtrati per fornitore + brand
            ColoriDistinti = await queryPerColore
                .Where(a => a.Colore != null && a.Colore != "")
                .Select(a => a.Colore)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(),

            // Taglie — filtrate per fornitore + brand + colore
            TaglieDistinte = await queryPerTaglia
                .Where(a => a.Taglia != null && a.Taglia != "")
                .Select(a => a.Taglia)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync()
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, string fornitore)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Errore"] = "Seleziona un file da importare.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(fornitore))
        {
            TempData["Errore"] = "Seleziona un fornitore.";
            return RedirectToAction(nameof(Index));
        }

        using var stream = file.OpenReadStream();
        var risultato = await _importService.ImportaFileAsync(stream, file.FileName, fornitore);

        TempData["Nuovi"] = risultato.Nuovi;
        TempData["GiaPresenti"] = risultato.GiaPresenti;
        TempData["Errori"] = risultato.Errori;

        if (risultato.DettagliErrori.Count > 0)
            TempData["DettagliErrori"] = string.Join("|", risultato.DettagliErrori.Take(50));

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetArticoliChunk(
        int start = 0,
        int size = 200,
        string? fornitore = null,
        string? brand = null,
        string? articolo = null,
        string? colore = null,
        string? taglia = null)
    {
        if (size <= 0) size = 200;
        if (size > 1000) size = 1000;
        if (start < 0) start = 0;

        var query = _db.ArticoliFornitori
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(fornitore))
            query = query.Where(a => a.Fornitore == fornitore);

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(a => a.Brand == brand);

        if (!string.IsNullOrWhiteSpace(articolo))
            query = query.Where(a => a.Articolo.Contains(articolo));

        if (!string.IsNullOrWhiteSpace(colore))
            query = query.Where(a => a.Colore == colore);

        if (!string.IsNullOrWhiteSpace(taglia))
            query = query.Where(a => a.Taglia == taglia);

        var total = await query.CountAsync();

        var rows = await query
            .OrderByDescending(a => a.DataImport)
            .Skip(start)
            .Take(size)
            .Select(a => new
            {
                a.Id,
                a.Articolo,
                a.Taglia,
                a.Colore,
                a.Fornitore,
                a.Brand,
                a.EAN,
                a.SKU
            })
            .ToListAsync();

        return Json(new
        {
            total,
            start,
            size,
            rows
        });
    }

    [HttpPost]
    public async Task<IActionResult> InviaSelezionati([FromBody] List<int> ids)
    {
        if (ids == null || ids.Count == 0)
            return BadRequest("Nessun articolo selezionato.");

        var articoli = await _db.ArticoliFornitori
            .AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .ToListAsync();

        if (articoli.Count == 0)
            return BadRequest("Articoli non trovati.");

        var count = await _interscambioService.InviaAlInterscambioAsync(articoli);

        return Json(new
        {
            success = true,
            inviati = count
        });
    }
}