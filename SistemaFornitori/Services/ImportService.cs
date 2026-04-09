using OfficeOpenXml;
using SistemaFornitori.Data;
using SistemaFornitori.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SistemaFornitori.Services;

public class ImportService
{
    private const int BatchSize = 1000;

    private readonly AppDbContext _db;
    private readonly MappingService _mappingService;
    private readonly ILogger<ImportService> _logger;

    public ImportService(AppDbContext db, MappingService mappingService, ILogger<ImportService> logger)
    {
        _db = db;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<ImportResult> ImportaFileAsync(Stream fileStream, string fileName, string fornitore)
    {
        var result = new ImportResult();

        var mapping = await _mappingService.GetMappingAsync(fornitore);
        if (mapping == null)
        {
            result.DettagliErrori.Add($"Mapping JSON non trovato per '{fornitore}'.");
            result.Errori = 1;
            return result;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        try
        {
            switch (extension)
            {
                case ".xlsx":
                case ".xls":
                    await ProcessExcelAsync(fileStream, mapping, fornitore, result);
                    break;

                case ".csv":
                    await ProcessCsvAsync(fileStream, mapping, fornitore, result);
                    break;

                case ".json":
                    await ProcessJsonAsync(fileStream, mapping, fornitore, result);
                    break;

                default:
                    result.DettagliErrori.Add($"Formato '{extension}' non supportato.");
                    result.Errori = 1;
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore import file {File}", fileName);
            result.DettagliErrori.Add($"Errore nel parsing: {ex.Message}");
            result.Errori++;
        }

        return result;
    }

    private async Task ProcessExcelAsync(Stream fileStream, SupplierMapping mapping, string fornitore, ImportResult result)
    {
        ExcelPackage.License.SetNonCommercialOrganization("SistemaFornitori");

        using var package = new ExcelPackage(fileStream);

        var worksheets = !string.IsNullOrWhiteSpace(mapping.SheetName)
            ? new[] { package.Workbook.Worksheets[mapping.SheetName] }
            : package.Workbook.Worksheets.ToArray();

        var batch = new List<ArticoloFornitore>(BatchSize);
        var batchEans = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sheet in worksheets)
        {
            if (sheet == null) continue;

            var headerRowIndex = mapping.HeaderRow + 1;
            var rowCount = sheet.Dimension?.Rows ?? 0;
            if (rowCount <= headerRowIndex) continue;

            var columnIndexes = RisolviIndiciColonne(sheet, headerRowIndex, mapping.Columns);
            if (!columnIndexes.ContainsKey("EAN")) continue;

            for (int row = headerRowIndex + 1; row <= rowCount; row++)
            {
                result.TotaleRigheFile++;

                try
                {
                    var articolo = CreaArticoloDaExcel(sheet, row, columnIndexes, mapping, fornitore);
                    if (articolo == null) continue;

                    if (!batchEans.Add(articolo.EAN))
                    {
                        result.GiaPresenti++;
                        continue;
                    }

                    batch.Add(articolo);

                    if (batch.Count >= BatchSize)
                    {
                        await SalvaBatchAsync(batch, result);
                        batch.Clear();
                        batchEans.Clear();
                    }
                }
                catch (Exception ex)
                {
                    result.Errori++;
                    result.DettagliErrori.Add($"Riga {row}: {ex.Message}");
                    _logger.LogWarning(ex, "Errore Excel riga {Row}", row);
                }
            }
        }

        if (batch.Count > 0)
            await SalvaBatchAsync(batch, result);
    }

    private async Task ProcessCsvAsync(Stream fileStream, SupplierMapping mapping, string fornitore, ImportResult result)
    {
        using var reader = new StreamReader(fileStream);

        var lines = new List<string>();
        while (await reader.ReadLineAsync() is { } line)
            lines.Add(line);

        if (lines.Count <= mapping.HeaderRow)
            throw new Exception("Il file CSV non contiene abbastanza righe.");

        var separator = lines[mapping.HeaderRow].Contains(';') ? ';' : ',';
        var headers = lines[mapping.HeaderRow]
            .Split(separator)
            .Select(h => h.Trim().Trim('"'))
            .ToArray();

        var columnIndexes = new Dictionary<string, int>();
        foreach (var (campo, nomeColonna) in mapping.Columns)
        {
            var index = Array.FindIndex(headers, h => h.Equals(nomeColonna, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
                columnIndexes[campo] = index;
        }

        if (!columnIndexes.ContainsKey("EAN"))
            throw new Exception("Colonna EAN non trovata nel CSV.");

        var batch = new List<ArticoloFornitore>(BatchSize);
        var batchEans = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = mapping.HeaderRow + 1; i < lines.Count; i++)
        {
            result.TotaleRigheFile++;

            try
            {
                var values = lines[i]
                    .Split(separator)
                    .Select(v => v.Trim().Trim('"'))
                    .ToArray();

                var articolo = CreaArticoloDaCsv(values, columnIndexes, mapping, fornitore);
                if (articolo == null) continue;

                if (!batchEans.Add(articolo.EAN))
                {
                    result.GiaPresenti++;
                    continue;
                }

                batch.Add(articolo);

                if (batch.Count >= BatchSize)
                {
                    await SalvaBatchAsync(batch, result);
                    batch.Clear();
                    batchEans.Clear();
                }
            }
            catch (Exception ex)
            {
                result.Errori++;
                result.DettagliErrori.Add($"Riga CSV {i + 1}: {ex.Message}");
                _logger.LogWarning(ex, "Errore CSV riga {Row}", i + 1);
            }
        }

        if (batch.Count > 0)
            await SalvaBatchAsync(batch, result);
    }

    private async Task ProcessJsonAsync(Stream fileStream, SupplierMapping mapping, string fornitore, ImportResult result)
    {
        using var reader = new StreamReader(fileStream);
        var jsonText = await reader.ReadToEndAsync();

        var jsonArray = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(jsonText)
            ?? throw new Exception("Il file JSON non contiene un array valido.");

        var batch = new List<ArticoloFornitore>(BatchSize);
        var batchEans = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var jsonObj in jsonArray)
        {
            result.TotaleRigheFile++;

            try
            {
                var articolo = CreaArticoloDaJson(jsonObj, mapping, fornitore);
                if (articolo == null) continue;

                if (!batchEans.Add(articolo.EAN))
                {
                    result.GiaPresenti++;
                    continue;
                }

                batch.Add(articolo);

                if (batch.Count >= BatchSize)
                {
                    await SalvaBatchAsync(batch, result);
                    batch.Clear();
                    batchEans.Clear();
                }
            }
            catch (Exception ex)
            {
                result.Errori++;
                result.DettagliErrori.Add($"Riga JSON: {ex.Message}");
                _logger.LogWarning(ex, "Errore JSON");
            }
        }

        if (batch.Count > 0)
            await SalvaBatchAsync(batch, result);
    }

    private ArticoloFornitore? CreaArticoloDaExcel(ExcelWorksheet sheet,int row,Dictionary<string, int> columnIndexes,SupplierMapping mapping,string fornitore)
    {
        var eanRaw = LeggiCella(sheet, row, columnIndexes, "EAN", mapping.Transforms);
        var ean = NormalizzaEan(eanRaw);
        if (ean == null) return null;

        var articolo = new ArticoloFornitore
        {
            EAN = ean,
            Articolo = LeggiCella(sheet, row, columnIndexes, "Articolo", mapping.Transforms),
            Fornitore = fornitore,
            Brand = LeggiCella(sheet, row, columnIndexes, "Brand", mapping.Transforms),
            Taglia = LeggiCella(sheet, row, columnIndexes, "Taglia", mapping.Transforms),
            Colore = LeggiCella(sheet, row, columnIndexes, "Colore", mapping.Transforms),
            SKU = LeggiCella(sheet, row, columnIndexes, "SKU", mapping.Transforms),
            DataImport = DateTime.Now
        };

        if (columnIndexes.TryGetValue("Prezzo", out var prezzoCol))
        {
            var cellValue = sheet.Cells[row, prezzoCol].Value;
            if (cellValue != null)
            {
                if (cellValue is double d)
                    articolo.Prezzo = (decimal)d;
                else if (cellValue is decimal dec)
                    articolo.Prezzo = dec;
                else if (decimal.TryParse(cellValue.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var prezzo))
                    articolo.Prezzo = prezzo;
            }
        }

        return articolo;
    }

    private ArticoloFornitore? CreaArticoloDaCsv(string[] values,Dictionary<string, int> columnIndexes,SupplierMapping mapping,string fornitore)
    {
        string ReadValue(string campo)
        {
            if (!columnIndexes.TryGetValue(campo, out var index)) return string.Empty;
            var value = index < values.Length ? values[index] : string.Empty;
            return ApplicaTrasformazione(value, campo, mapping.Transforms);
        }

        var ean = NormalizzaEan(ReadValue("EAN"));
        if (ean == null) return null;

        var articolo = new ArticoloFornitore
        {
            EAN = ean,
            Articolo = ReadValue("Articolo"),
            Fornitore = fornitore,
            Brand = ReadValue("Brand"),
            Taglia = ReadValue("Taglia"),
            Colore = ReadValue("Colore"),
            SKU = ReadValue("SKU"),
            DataImport = DateTime.Now
        };

        var prezzoStr = ReadValue("Prezzo");
        if (decimal.TryParse(prezzoStr, out var prezzo))
            articolo.Prezzo = prezzo;

        return articolo;
    }

    private ArticoloFornitore? CreaArticoloDaJson(Dictionary<string, JsonElement> jsonObj,SupplierMapping mapping,string fornitore)
    {
        string ReadValue(string campo)
        {
            if (!mapping.Columns.TryGetValue(campo, out var sourceColumn)) return string.Empty;
            if (!jsonObj.TryGetValue(sourceColumn, out var element)) return string.Empty;
            return ApplicaTrasformazione(element.ToString(), campo, mapping.Transforms);
        }

        var ean = NormalizzaEan(ReadValue("EAN"));
        if (ean == null) return null;

        var articolo = new ArticoloFornitore
        {
            EAN = ean,
            Articolo = ReadValue("Articolo"),
            Fornitore = fornitore,
            Brand = ReadValue("Brand"),
            Taglia = ReadValue("Taglia"),
            Colore = ReadValue("Colore"),
            SKU = ReadValue("SKU"),
            DataImport = DateTime.Now
        };

        var prezzoStr = ReadValue("Prezzo");
        if (decimal.TryParse(prezzoStr, out var prezzo))
            articolo.Prezzo = prezzo;

        return articolo;
    }

    private async Task SalvaBatchAsync(List<ArticoloFornitore> batch, ImportResult result)
    {
        if (batch.Count == 0) return;

        var eanBatch = batch
            .Select(x => x.EAN)
            .Distinct()
            .ToList();

        var eanGiaPresenti = await _db.ArticoliFornitori
            .AsNoTracking()
            .Where(a => eanBatch.Contains(a.EAN))
            .Select(a => a.EAN)
            .ToListAsync();

        var setGiaPresenti = eanGiaPresenti.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var daInserire = batch
            .Where(x => !setGiaPresenti.Contains(x.EAN))
            .ToList();

        result.GiaPresenti += batch.Count - daInserire.Count;
        result.Nuovi += daInserire.Count;

        if (daInserire.Count == 0) return;

        var oldAutoDetect = _db.ChangeTracker.AutoDetectChangesEnabled;
        _db.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            _db.ArticoliFornitori.AddRange(daInserire);
            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();
        }
        finally
        {
            _db.ChangeTracker.AutoDetectChangesEnabled = oldAutoDetect;
        }
    }

    private Dictionary<string, int> RisolviIndiciColonne(ExcelWorksheet sheet,int headerRow,Dictionary<string, string> columnMapping)
    {
        var indexes = new Dictionary<string, int>();
        var colCount = sheet.Dimension?.Columns ?? 0;

        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int col = 1; col <= colCount; col++)
        {
            var headerValue = sheet.Cells[headerRow, col].Text?.Trim();
            if (!string.IsNullOrEmpty(headerValue))
                headers[headerValue] = col;
        }

        foreach (var (campo, nomeColonna) in columnMapping)
        {
            if (headers.TryGetValue(nomeColonna, out var colIndex))
                indexes[campo] = colIndex;
        }

        return indexes;
    }

    private string LeggiCella(ExcelWorksheet sheet,int row,Dictionary<string, int> columnIndexes,string campo,Dictionary<string, string> transforms)
    {
        if (!columnIndexes.TryGetValue(campo, out var colIndex))
            return string.Empty;

        var value = sheet.Cells[row, colIndex].Text?.Trim() ?? string.Empty;
        return ApplicaTrasformazione(value, campo, transforms);
    }

    private string ApplicaTrasformazione(string value, string campo, Dictionary<string, string> transforms)
    {
        if (transforms.TryGetValue(campo, out var transform))
        {
            value = transform.ToUpperInvariant() switch
            {
                "UPPER" => value.ToUpperInvariant(),
                "LOWER" => value.ToLowerInvariant(),
                "TRIM" => value.Trim(),
                _ => value
            };
        }

        return value;
    }

    private string? NormalizzaEan(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length != 13)
            return null;

        return digits;
    }
}