namespace SistemaFornitori.Models;


// Risultato di un import 
public class ImportResult
{
    public int Nuovi { get; set; }
    public int GiaPresenti { get; set; }
    public int Errori { get; set; }
    public List<string> DettagliErrori { get; set; } = new();
    public int TotaleRigheFile { get; set; }
}
