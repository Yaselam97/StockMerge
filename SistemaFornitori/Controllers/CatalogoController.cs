using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFornitori.Data;
using SistemaFornitori.Services;
using SistemaFornitori.ViewModels;

namespace SistemaFornitori.Controllers;

public class CatalogoController : Controller
{
    private readonly AppDbContext _db;
    private readonly InterscambioService _interscambioService;
    private readonly ExportService _exportService;

    public CatalogoController(AppDbContext db, InterscambioService interscambioService, ExportService exportService)
    {
        _db = db;
        _interscambioService = interscambioService;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index()
    {
        var eanInviati = await _interscambioService.GetEanInviatiAsync();

        var vm = new CatalogoViewModel
        {
            Articoli = await _db.ArticoliFornitori
                .AsNoTracking()
                .Where(a => eanInviati.Contains(a.EAN))
                .OrderBy(a => a.Brand)
                .ThenBy(a => a.Articolo)
                .ToListAsync()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var articoli = await GetArticoliInviatiAsync();
        var bytes = _exportService.GeneraExcel(articoli);
        var fileName = $"Articoli_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> StampaPdf()
    {
        var articoli = await GetArticoliInviatiAsync();
        var bytes = _exportService.GeneraPdf(articoli);
        var fileName = $"Articoli_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    private async Task<List<Models.ArticoloFornitore>> GetArticoliInviatiAsync()
    {
        var eanInviati = await _interscambioService.GetEanInviatiAsync();
        return await _db.ArticoliFornitori
            .AsNoTracking()
            .Where(a => eanInviati.Contains(a.EAN))
            .OrderBy(a => a.Brand)
            .ThenBy(a => a.Articolo)
            .ToListAsync();
    }
}