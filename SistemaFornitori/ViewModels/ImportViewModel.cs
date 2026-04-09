using SistemaFornitori.Models;

namespace SistemaFornitori.ViewModels;

public class ImportViewModel
{
    public List<string> Fornitori { get; set; } = new();
    public string? FornitoreSelezionato { get; set; }
    public ImportResult? Risultato { get; set; }

    // La tabella non viene più renderizzata tutta server-side
    public List<ArticoloFornitore> Articoli { get; set; } = new();

    public string? FiltroFornitore { get; set; }
    public string? FiltroBrand { get; set; }
    public string? FiltroArticolo { get; set; }
    public string? FiltroColore { get; set; }
    public string? FiltroTaglia { get; set; }


    public List<string> FornitoriDistinti { get; set; } = new();
    public List<string> BrandDistinti { get; set; } = new();
    public List<string> ColoriDistinti { get; set; } = new();
    public List<string> TaglieDistinte { get; set; } = new();

    public string? Errore { get; set; }
}