# \# StockMerge

# 

# StockMerge è un sistema di integrazione cataloghi fornitori progettato per importare, normalizzare, unificare e gestire dati prodotto provenienti da più fornitori all’interno di una struttura unica e coerente.

# 

# Il progetto nasce per risolvere un problema operativo molto comune: ogni fornitore invia i propri listini o cataloghi in formati diversi, con colonne diverse, strutture differenti e qualità del dato non uniforme. Questo rende il consolidamento manuale lento, ripetitivo e soggetto a errori.

# 

# StockMerge centralizza questo processo permettendo di importare file fornitore, applicare mapping personalizzati, normalizzare i dati, rilevare duplicati e salvare tutto in un catalogo interno unificato.

# 

# \---

# 

# \## Perché è stato creato

# 

# Nei flussi aziendali reali, i dati dei fornitori arrivano spesso tramite file Excel o CSV, ma ogni fornitore usa un’impostazione diversa:

# 

# \- nomi colonne differenti

# \- strutture file non uniformi

# \- campi mancanti o incoerenti

# \- prodotti duplicati

# \- necessità di normalizzazione manuale prima dell’utilizzo interno

# 

# Lo scopo di StockMerge è standardizzare e automatizzare questo processo.

# 

# Invece di pulire e unire manualmente i file ogni volta, il sistema fornisce un modo strutturato per:

# 

# \- definire mapping specifici per fornitore

# \- leggere file esterni

# \- trasformare i dati in un modello comune

# \- individuare duplicati

# \- salvare risultati puliti nel database

# \- preparare i dati per i flussi interni di catalogo e stock

# 

# \---

# 

# \## Obiettivi principali

# 

# \- semplificare l’import dei prodotti fornitore

# \- normalizzare dati esterni eterogenei

# \- ridurre il lavoro manuale su Excel

# \- evitare inserimenti duplicati nel catalogo

# \- creare una base scalabile per future integrazioni fornitore

# \- migliorare affidabilità e manutenibilità dei flussi di importazione

# 

# \---

# 

# \## Funzionalità principali

# 

# \- importazione file fornitore da Excel

# \- configurazione mapping per fornitore tramite file JSON

# \- risoluzione dinamica delle colonne in base alle regole di mapping

# \- normalizzazione e trasformazione dei dati

# \- rilevamento duplicati basato su EAN

# \- salvataggio in un catalogo prodotto unificato

# \- tracciamento risultato importazione

# \- logging import per monitoraggio e troubleshooting

# \- gestione fornitori con stato attivo/non attivo

# 

# \---

# 

# \## Struttura del progetto

# 

# L’applicazione è organizzata per mantenere le responsabilità chiare e il codice facilmente manutenibile.

# 

# \### Cartelle principali

# 

# \- `Controllers/`  

# &#x20; Gestisce le richieste HTTP e il flusso dell’applicazione.

# 

# \- `Models/`  

# &#x20; Contiene le entità di dominio come fornitori, prodotti importati, log e modelli di integrazione.

# 

# \- `ViewModels/`  

# &#x20; Contiene i modelli dedicati all’interfaccia utente e all’interazione con le pagine.

# 

# \- `Services/`  

# &#x20; Contiene la logica principale di business, inclusi il caricamento dei mapping e l’elaborazione degli import.

# 

# \- `Data/`  

# &#x20; Contiene il contesto Entity Framework e la configurazione della persistenza.

# 

# \- `Mappings/`  

# &#x20; Contiene i file JSON che definiscono come interpretare i file di ciascun fornitore.

# 

# \- `Views/`  

# &#x20; Viste Razor ASP.NET MVC.

# 

# \- `wwwroot/`  

# &#x20; File statici.

# 

# \---

# 

# \## Come funziona

# 

# \### 1. Configurazione fornitore

# Ogni fornitore viene registrato nel sistema con il proprio file di mapping.

# 

# Esempi:

# \- Clique → `clique.json`

# \- Craft → `craft.json`

# \- ProJob → `projob.json`

# 

# \### 2. Definizione del mapping

# Ogni file di mapping descrive come leggere correttamente il file del fornitore:

# 

# \- tipo file

# \- riga di intestazione

# \- nome foglio

# \- colonne sorgente

# \- valori fissi

# \- regole di trasformazione

# 

# Questo rende il processo di importazione flessibile, riutilizzabile ed estendibile senza dover codificare ogni formato fornitore direttamente nel codice.

# 

# \### 3. Importazione file

# Il sistema legge il file caricato tramite EPPlus, individua le colonne necessarie in base alla configurazione del mapping ed estrae i dati prodotto.

# 

# \### 4. Normalizzazione dei dati

# I valori importati possono essere trasformati con regole come:

# 

# \- `TRIM`

# \- `UPPER`

# \- `LOWER`

# 

# In questo modo i dati vengono uniformati prima del salvataggio.

# 

# \### 5. Controllo duplicati

# I prodotti vengono verificati tramite EAN prima dell’inserimento, evitando duplicazioni nel catalogo.

# 

# \### 6. Salvataggio e logging

# I record validi vengono salvati nel database, mentre il sistema registra statistiche di importazione e log utili per diagnosi e controllo.

# 

# \---

# 

# \## Panoramica del modello dati

# 

# \### `Fornitore`

# Rappresenta un fornitore.

# 

# Campi principali:

# \- `Nome`

# \- `MappingFile`

# \- `Attivo`

# \- `DataCreazione`

# 

# \### `ArticoloFornitore`

# Rappresenta un articolo unificato importato da un fornitore.

# 

# Campi principali:

# \- `EAN`

# \- `Articolo`

# \- `Taglia`

# \- `Colore`

# \- `SKU`

# \- `Fornitore`

# \- `Prezzo`

# \- `DataImport`

# \- `Selezionato`

# \- `Quantita`

# 

# \### `ImportLog`

# Memorizza informazioni tecniche e operative relative all’esecuzione degli import.

# 

# \### `ImportResult`

# Restituisce il risultato dell’elaborazione, ad esempio numero righe processate, inserite, duplicate o in errore.

# 

# \---

# 

# \## Tecnologie utilizzate

# 

# \- \*\*ASP.NET Core MVC\*\*

# \- \*\*C#\*\*

# \- \*\*Entity Framework Core\*\*

# \- \*\*SQL Server\*\*

# \- \*\*EPPlus\*\*

# \- \*\*Dapper\*\*

# \- \*\*Mapping JSON configurabili\*\*

# 

# \---

# 

# \## Scelte progettuali

# 

# Il progetto è stato sviluppato volutamente come un’unica applicazione ASP.NET Core MVC, per mantenere lo sviluppo rapido, leggibile e facilmente manutenibile.

# 

# L’architettura punta alla praticità:

# 

# \- MVC per una struttura chiara e produttiva

# \- Services per separare la logica di business

# \- file JSON per rendere i mapping estendibili

# \- EF Core per la persistenza

# \- EPPlus per la gestione dei file Excel

# 

# Questo rende l’applicazione semplice da evolvere, ma già adatta a un utilizzo reale in ambito operativo.

# 

# \---

# 

# \## Benefici

# 

# StockMerge permette di trasformare file fornitore frammentati in un processo strutturato e affidabile di consolidamento catalogo.

# 

# Benefici principali:

# 

# \- meno lavoro manuale

# \- meno errori di inserimento dati

# \- onboarding più rapido di nuovi fornitori

# \- consolidamento cataloghi più semplice

# \- logica di import più manutenibile

# \- base scalabile per future integrazioni

# 

# \---

# 

# \## Possibili evoluzioni future

# 

# Possibili sviluppi futuri:

# 

# \- miglioramento supporto CSV

# \- regole di validazione avanzate

# \- dashboard storico importazioni

# \- funzionalità di export

# \- elaborazione asincrona per file grandi

# \- gestione ruoli e permessi

# \- API per integrazioni esterne

# \- sincronizzazione stock con sistemi terzi

# 

# \---

# 

# \## Stato del progetto

# 

# Questo progetto è uno strumento operativo interno progettato per supportare il consolidamento dei cataloghi fornitore e i flussi legati a stock e articoli.

# 

# Attualmente rappresenta una base solida, pronta per essere estesa con funzionalità aggiuntive e integrazioni future.

# 

# \---

# 

# \## Autore

# 

# \*\*Yassine El Amrati\*\*

