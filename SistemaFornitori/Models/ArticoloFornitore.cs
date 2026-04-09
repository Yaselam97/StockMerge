namespace SistemaFornitori.Models;

public class ArticoloFornitore
{
    public int Id { get; set; }
    public string EAN { get; set; } = string.Empty;
    public string Articolo { get; set; } = string.Empty;
    public string Fornitore { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Taglia { get; set; } = string.Empty;
    public string Colore { get; set; } = string.Empty;
    
    public string SKU { get; set; } = string.Empty;
    public decimal? Prezzo { get; set; }
    public DateTime DataImport { get; set; } = DateTime.Now;
}