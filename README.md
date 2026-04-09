# Sistema Import Fornitori

Sistema web interno per importare cataloghi prodotti da file Excel/CSV dei distributori, eliminare i duplicati tramite codice EAN13, e costruire un catalogo unificato da cui selezionare gli articoli da inviare al gestionale EMSWin tramite il database di interscambio.

## Il Problema

Un distributore di articoli sportivi riceve file Excel dai propri fornitori con migliaia di articoli ogni stagione. Ogni fornitore ha un formato diverso, i file contengono sia articoli nuovi che vecchi, e lo stesso EAN può comparire in fogli diversi. Gestire tutto manualmente è lento e soggetto a errori.

## La Soluzione

Un'applicazione ASP.NET Core MVC che:

- **Importa** file Excel/CSV/JSON da qualsiasi distributore, leggendo tutti i fogli in automatico
- **Elimina i duplicati** usando l'EAN13 come chiave univoca
- **Configura ogni distributore** con un file JSON di mapping — aggiungere un nuovo distributore non richiede modifiche al codice
- **Seleziona** gli articoli da inviare con filtri a cascata (fornitore → brand → colore → taglia)
- **Scrive** gli articoli selezionati nel database di interscambio (`005_AnArticoli` + `005_AnBarcode`), da cui ConnecTilog li porta in EMSWin
- **Esporta** la lista finale in PDF e Excel

## Architettura

Progetto singolo ASP.NET Core MVC (.NET 10), struttura semplice e diretta:

```
SistemaFornitori/
├── Controllers/
│   ├── ImportController.cs        # Upload, filtri, invio al DB interscambio
│   └── CatalogoController.cs      # Lista finale, export PDF/Excel
├── Models/
│   ├── ArticoloFornitore.cs       # Entità principale — catalogo unificato
│   ├── ImportResult.cs            # Risultato di un'operazione di import
│   ├── InterscambioModels.cs      # Modelli per 005_AnArticoli e 005_AnBarcode
│   └── SupplierMapping.cs         # Configurazione mapping colonne per distributore
├── ViewModels/
│   ├── ImportViewModel.cs         # Dati per la pagina Import
│   └── CatalogoViewModel.cs       # Dati per la pagina Lista Finale
├── Services/
│   ├── ImportService.cs           # Parsing file + dedup EAN + salvataggio DB
│   ├── MappingService.cs          # Caricamento file JSON di configurazione
│   ├── InterscambioService.cs     # Scrittura/lettura DB EMS_Premi_Interscambio
│   └── ExportService.cs           # Generazione PDF e Excel
├── Data/
│   └── AppDbContext.cs            # Entity Framework Core — DB SistemaFornitori
├── Mappings/
│   ├── distributore_1.json        # Mapping colonne per il distributore 1
│   ├── distributore_2.json        # Mapping colonne per il distributore 2
│   └── distributore_3.json        # Mapping colonne per il distributore 3
├── Views/
│   ├── Import/Index.cshtml        # Pagina upload + tabella con scroll infinito
│   ├── Catalogo/Index.cshtml      # Lista finale con export
│   └── Shared/_Layout.cshtml      # Layout condiviso
├── wwwroot/css/site.css           # Stili custom (no Bootstrap)
├── Program.cs                     # Entry point e registrazione DI
└── appsettings.json               # Connection strings
```

## Flusso Operativo

```
1. IMPORT
   File Excel → Upload sul sito → Scegli distributore
                                        ↓
   Parsing automatico (tutti i fogli) → Dedup EAN13
                                        ↓
   Salvataggio nel DB SistemaFornitori (tabella ArticoliFornitori)

2. SELEZIONE + INVIO
   Tabella articoli con filtri a cascata → Seleziona con checkbox
                                        ↓
   "Invia selezionati" → Scrittura in 005_AnArticoli + 005_AnBarcode
                          (DB EMS_Premi_Interscambio)
                                        ↓
   ConnecTilog legge le tabelle 005_* → Porta in EMSWin

3. LISTA FINALE
   Mostra solo articoli già inviati al DB interscambio
                                        ↓
   Stampa PDF / Export Excel
```

## Database

Il sistema utilizza due database sullo stesso SQL Server:

**`SistemaFornitori`** — database interno, gestito da EF Core:
- `ArticoliFornitori` — catalogo unificato con EAN UNIQUE, brand, fornitore, taglia, colore, SKU, prezzo

**`EMS_Premi_Interscambio_test`** — database di interscambio esistente, scritto via Dapper:
- `005_AnArticoli` — anagrafica articoli per il cliente 005 (stessa struttura delle 002, 006, 008)
- `005_AnBarcode` — barcode associati agli articoli
- `005_AnCliForn` — anagrafica clienti/fornitori (inserimento manuale)

## Mapping Distributori

Ogni distributore ha un file JSON nella cartella `Mappings/`. Il file definisce la corrispondenza tra le colonne del file Excel e i campi del sistema:

```json
{
  "Fornitore": "Distributore 1",
  "FileType": "xlsx",
  "HeaderRow": 0,
  "SheetName": null,
  "Columns": {
    "EAN": "Ean",
    "Articolo": "Product name (it)",
    "Taglia": "Size name",
    "Colore": "Color (it)",
    "SKU": "Sku",
    "Prezzo": "Prezzo netto rivenditore",
    "Brand": "Brand"
  },
  "Transforms": {
    "EAN": "TRIM",
    "Taglia": "UPPER"
  }
}
```

**Per aggiungere un nuovo distributore**: copia un JSON esistente, rinomina il file, adatta i nomi delle colonne. Zero codice da toccare.

## Stack Tecnologico

- **ASP.NET Core MVC** (.NET 10) — framework web
- **Entity Framework Core** — ORM per il DB interno SistemaFornitori
- **Dapper** — micro-ORM per le scritture nel DB interscambio (tabelle 005_* con struttura fissa)
- **EPPlus** — parsing file Excel (lettura multi-foglio)
- **QuestPDF** — generazione PDF
- **SQL Server** — database

## Scelte Tecniche

**Perché un progetto singolo e non 3 layer?**
Il sistema ha 2 use case (importa file, mostra catalogo). Separare in Core/Infrastructure/Web per un progetto così semplice aggiunge complessità senza beneficio.

**Perché EF Core per un DB e Dapper per l'altro?**
Il DB `SistemaFornitori` lo gestiamo noi — EF Core con migration è perfetto. Le tabelle `005_*` nel DB interscambio hanno una struttura fissa definita da ConnecTilog — un secondo DbContext sarebbe eccessivo per semplici INSERT.

**Perché scroll infinito nella tabella?**
Con ~68.000 articoli, renderizzare tutto server-side bloccherebbe il browser. Il caricamento a chunk da 200 righe con scroll infinito mantiene la pagina reattiva.

**Perché i filtri sono a cascata?**
Se scegli un fornitore, i brand nel dropdown mostrano solo quelli di quel fornitore. Se poi scegli un brand, i colori mostrano solo quelli di quel brand. Evita confusione con migliaia di valori.

**Perché nessuna tabella Fornitori nel DB?**
I distributori sono definiti nei file JSON. Aggiungerne uno è copiare un file, non fare una INSERT. Meno tabelle = meno complessità.

## Configurazione

### Prerequisiti

- .NET 10 SDK
- SQL Server (accesso a due database)
- Visual Studio 2022+ (consigliato)

### Setup

1. Clona il repository
2. Aggiorna le connection strings in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "SistemaFornitori": "Server=TUO_SERVER;Database=SistemaFornitori;User Id=sa;Password=TUA_PASSWORD;TrustServerCertificate=True;",
     "Interscambio": "Server=TUO_SERVER;Database=EMS_Premi_Interscambio_test;User Id=sa;Password=TUA_PASSWORD;TrustServerCertificate=True;"
   }
   ```
3. Crea il database `SistemaFornitori` su SQL Server
4. In Package Manager Console:
   ```
   Add-Migration InitialCreate
   Update-Database
   ```
5. Assicurati che le tabelle `005_AnArticoli`, `005_AnBarcode` e `005_AnCliForn` esistano nel DB interscambio (stessa struttura delle 006_*)
6. Aggiorna i file JSON nella cartella `Mappings/` con i nomi reali delle colonne dei tuoi file Excel
7. F5 per avviare

### NuGet Packages

- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `EPPlus`
- `Dapper`
- `QuestPDF`

## Struttura Tabella ArticoliFornitori

| Campo | Tipo | Note |
|-------|------|------|
| Id | int | PK, auto-increment |
| EAN | nvarchar(13) | UNIQUE — chiave di dedup |
| Articolo | nvarchar(200) | Nome prodotto |
| Fornitore | nvarchar(100) | Nome distributore (dal JSON) |
| Brand | nvarchar(100) | Brand dal file (Clique, Craft, ecc.) |
| Taglia | nvarchar(20) | |
| Colore | nvarchar(50) | |
| SKU | nvarchar(50) | Codice articolo fornitore |
| Prezzo | decimal(10,2) | Prezzo netto (opzionale) |
| DataImport | datetime2 | Default GETDATE() |

## Sviluppo Futuro

- **Fase 2**: Gestione ordini (tabelle `005_OrdIngressoTes`, `005_OrdIngressoRig`, `005_OrdUscitaTES`, `005_OrdUscitaRIG`)
- **005_AnCliForn**: Inserimento manuale fornitori nell'anagrafica interscambio
- Autenticazione utenti
- Log delle operazioni di invio al DB interscambio

---

*Sviluppato presso TiLog — 2026*
