using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFornitori.Models;

namespace SistemaFornitori.Services;

public class ExportService
{
    public byte[] GeneraExcel(List<ArticoloFornitore> articoli)
    {
        ExcelPackage.License.SetNonCommercialOrganization("SistemaFornitori");
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Articoli");

        // Header
        var headers = new[] { "EAN", "Articolo", "Fornitore", "Brand", "Taglia", "Colore", "SKU" };
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cells[1, i + 1].Value = headers[i];
            sheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        // Dati
        for (int r = 0; r < articoli.Count; r++)
        {
            var art = articoli[r];
            sheet.Cells[r + 2, 1].Value = art.EAN;
            sheet.Cells[r + 2, 2].Value = art.Articolo;
            sheet.Cells[r + 2, 3].Value = art.Fornitore;
            sheet.Cells[r + 2, 4].Value = art.Brand;
            sheet.Cells[r + 2, 5].Value = art.Taglia;
            sheet.Cells[r + 2, 6].Value = art.Colore;
            sheet.Cells[r + 2, 7].Value = art.SKU;
        }

        sheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] GeneraPdf(List<ArticoloFornitore> articoli)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Lista Articoli").FontSize(18).Bold().FontColor("#2d5a3d");
                        col.Item().Text($"Generato il {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor("#999");
                        col.Item().Text($"{articoli.Count} articoli").FontSize(9).FontColor("#999");
                    });
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);   // EAN
                        columns.RelativeColumn(3);   // Articolo
                        columns.RelativeColumn(1.5f); // Fornitore
                        columns.RelativeColumn(1.5f); // Brand
                        columns.RelativeColumn(1);   // Taglia
                        columns.RelativeColumn(1.5f); // Colore
                        columns.RelativeColumn(2);   // SKU

                    });

                    // Header
                    table.Header(header =>
                    {
                        var headerStyle = TextStyle.Default.FontSize(8).Bold().FontColor("#fff");

                        header.Cell().Background("#2d5a3d").Padding(4).Text("EAN").Style(headerStyle);
                        header.Cell().Background("#2d5a3d").Padding(4).Text("Articolo").Style(headerStyle);
                        header.Cell().Background("#2d5a3d").Padding(4).Text("Fornitore").Style(headerStyle);
                        header.Cell().Background("#2d5a3d").Padding(4).Text("Brand").Style(headerStyle);
                        header.Cell().Background("#2d5a3d").Padding(4).Text("Taglia").Style(headerStyle);
                        header.Cell().Background("#2d5a3d").Padding(4).Text("Colore").Style(headerStyle);
                        header.Cell().Background("#2d5a3d").Padding(4).Text("SKU").Style(headerStyle);
                        
                    });

                    // Righe dati
                    var cellStyle = TextStyle.Default.FontSize(7);

                    for (int i = 0; i < articoli.Count; i++)
                    {
                        var art = articoli[i];
                        var bg = i % 2 == 0 ? "#ffffff" : "#f7f5f0";

                        table.Cell().Background(bg).Padding(3).Text(art.EAN).Style(cellStyle);
                        table.Cell().Background(bg).Padding(3).Text(art.Articolo).Style(cellStyle);
                        table.Cell().Background(bg).Padding(3).Text(art.Fornitore).Style(cellStyle);
                        table.Cell().Background(bg).Padding(3).Text(art.Brand).Style(cellStyle);
                        table.Cell().Background(bg).Padding(3).Text(art.Taglia).Style(cellStyle);
                        table.Cell().Background(bg).Padding(3).Text(art.Colore).Style(cellStyle);
                        table.Cell().Background(bg).Padding(3).Text(art.SKU).Style(cellStyle);
                        
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Pagina ").FontSize(8).FontColor("#999");
                    text.CurrentPageNumber().FontSize(8).FontColor("#999");
                    text.Span(" di ").FontSize(8).FontColor("#999");
                    text.TotalPages().FontSize(8).FontColor("#999");
                });
            });
        });

        return document.GeneratePdf();
    }
}