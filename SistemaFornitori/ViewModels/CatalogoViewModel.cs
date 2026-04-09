using SistemaFornitori.Models;

namespace SistemaFornitori.ViewModels;

public class CatalogoViewModel
{
    public List<ArticoloFornitore> Articoli { get; set; } = new();
    public int TotaleArticoli => Articoli.Count;
    public string? Errore { get; set; }
    public string? Messaggio { get; set; }
}