namespace SistemaFornitori.Models;

// Configurazione mapping colonne per un singolo fornitore.
// Viene deserializzata dal file JSON 
// Aggiungere un fornitore = aggiungere un JSON

public class SupplierMapping
{
    // Nome del fornitore
    public string Fornitore { get; set; } = string.Empty;

    // Formato file: "xlsx" o "csv" o "json"
    public string FileType { get; set; } = "xlsx";

    // Riga dell'header nel file Excel (0-based)
    public int HeaderRow { get; set; } = 0;

    // Nome del foglio Excel 
    public string? SheetName { get; set; }

    // Mapping colonne: chiave = campo destinazione, valore = nome colonna nel file.
    public Dictionary<string, string> Columns { get; set; } = new();


    // Valori fissi per tutti i record.
    public Dictionary<string, string> FixedValues { get; set; } = new();


    // Trasformazioni: "UPPER", "LOWER", "TRIM"
    public Dictionary<string, string> Transforms { get; set; } = new();
}