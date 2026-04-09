using System.Text.Json;
using SistemaFornitori.Models;

namespace SistemaFornitori.Services;


// Carica i file JSON di mapping dalla cartella Mappings.
// Aggiungere un fornitore = aggiungere un JSON

public class MappingService
{
    private readonly string _mappingsPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public MappingService(string mappingsPath)
    {
        _mappingsPath = mappingsPath;
    }

    public async Task<SupplierMapping?> GetMappingAsync(string fornitore)
    {
        var files = Directory.GetFiles(_mappingsPath, "*.json");
        var file = files.FirstOrDefault(f =>
            Path.GetFileNameWithoutExtension(f)
                .Equals(fornitore, StringComparison.OrdinalIgnoreCase));

        if (file == null) return null;

        var json = await File.ReadAllTextAsync(file);
        return JsonSerializer.Deserialize<SupplierMapping>(json, _jsonOptions);
    }

    public List<string> GetMappingsDisponibili()
    {
        return Directory.GetFiles(_mappingsPath, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(x => x)
            .ToList();
    }
}