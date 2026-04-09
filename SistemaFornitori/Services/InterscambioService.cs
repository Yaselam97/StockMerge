using Dapper;
using Microsoft.Data.SqlClient;
using SistemaFornitori.Models;

namespace SistemaFornitori.Services;

public class InterscambioService
{
    private readonly string _connectionString;
    private readonly ILogger<InterscambioService> _logger;

    public InterscambioService(IConfiguration config, ILogger<InterscambioService> logger)
    {
        _connectionString = config.GetConnectionString("Interscambio")
            ?? throw new InvalidOperationException("Connection string 'Interscambio' non configurata.");
        _logger = logger;
    }

    public async Task<int> InviaAlInterscambioAsync(List<ArticoloFornitore> articoli)
    {
        if (articoli.Count == 0) 
            return 0;

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            int inseriti = 0;
            var adesso = DateTime.Now;

            foreach (var art in articoli)
            {
                var sqlArticolo = @"
                    IF NOT EXISTS (SELECT 1 FROM [005_AnArticoli] WHERE codArticolo = @codArticolo AND codSoc = '005')
                    INSERT INTO [005_AnArticoli]
                        (codSoc, codArticolo, descrizione, CodUM, CodArtFornitore,
                         gestLotto, gestDtScadenza, CodGestMatricola, setBloccoInIngresso,
                         gestVariante1, gestVariante2, gestVariante3, gestVariante4,
                         gestVariante5, gestVariante6, gestVariante7, gestVariante8,
                         richiediRiservaPallet, BloccoIdPalletPrelievo,
                         Abilitato, Cancellato,
                         dataCreazione, dataUltModifica, utenteCreazione, utenteUltModifica)
                    VALUES
                        ('005', @codArticolo, @descrizione, 'PZ', @CodArtFornitore,
                         1, 0, 'NO', 0,
                         0, 0, 0, 0,
                         0, 0, 0, 0,
                         0, 0,
                         1, 0,
                         @dataCreazione, @dataUltModifica, @utente, @utente)";

                await connection.ExecuteAsync(sqlArticolo, new
                {
                    codArticolo = art.EAN,
                    descrizione = $"{art.Articolo} {art.Taglia} {art.Colore}".Trim(),
                    CodArtFornitore = art.SKU,
                    dataCreazione = adesso,
                    dataUltModifica = adesso,
                    utente = "SistemaFornitori"
                }, transaction);

                var sqlBarcode = @"
                    IF NOT EXISTS (SELECT 1 FROM [005_AnBarcode] WHERE Barcode = @Barcode AND codSoc = '005')
                    INSERT INTO [005_AnBarcode]
                        (codSoc, Barcode, codArticolo, QtaPerBarcode,
                         Abilitato, Cancellato,
                         dataCreazione, dataUltModifica, utenteCreazione, utenteUltModifica)
                    VALUES
                        ('005', @Barcode, @codArticolo, 1,
                         1, 0,
                         @dataCreazione, @dataUltModifica, @utente, @utente)";

                await connection.ExecuteAsync(sqlBarcode, new
                {
                    Barcode = art.EAN,
                    codArticolo = art.EAN,
                    dataCreazione = adesso,
                    dataUltModifica = adesso,
                    utente = "SistemaFornitori"
                }, transaction);

                inseriti++;
            }

            transaction.Commit();
            _logger.LogInformation("Scritti {Count} articoli nelle tabelle interscambio", inseriti);
            return inseriti;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Errore nella scrittura al DB interscambio");
            throw;
        }
    }

    public async Task<List<string>> GetEanInviatiAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT codArticolo FROM [005_AnArticoli] WHERE codSoc = '005' AND Abilitato = 1 AND Cancellato = 0";
        var risultati = await connection.QueryAsync<string>(sql);
        return risultati.ToList();
    }
}