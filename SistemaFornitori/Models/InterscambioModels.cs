using Microsoft.AspNetCore.Http.HttpResults;

namespace SistemaFornitori.Models;


// Record per 005_AnArticoli nel DB EMS_Premi_Interscambio.
// ConnecTilog legge questa tabella e la porta in EMSWin.

public class AnArticolo
{
    public string codSoc { get; set; } = "005";
    public string codArticolo { get; set; } = string.Empty;    // ← EAN
    public string descrizione { get; set; } = string.Empty;     // ← Articolo
    public string CodUM { get; set; } = string.Empty;           // ← Taglia
    public string CodArtFornitore { get; set; } = string.Empty; // ← SKU
    public int gestLotto { get; set; } = 1;
    public int gestDtScadenza { get; set; } = 0;
    public string CodGestMatricola { get; set; } = "NO";
    public int Abilitato { get; set; } = 1;
    public int Cancellato { get; set; } = 0;
}


// Record per 005_AnBarcode nel DB EMS_Premi_Interscambio.

public class AnBarcode
{
    public string codSoc { get; set; } = "005";
    public string Barcode { get; set; } = string.Empty;
    public string codArticolo { get; set; } = string.Empty;
    public int QtaPerBarcode { get; set; } = 1;
    public int Abilitato { get; set; } = 1;
    public int Cancellato { get; set; } = 0;
}
