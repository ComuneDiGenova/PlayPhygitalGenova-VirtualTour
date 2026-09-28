## Descrizione del modulo custom/phygital

Ogni pagina esposta tramite percorso web ha un nome interno definito all'interno di questo modulo personalizzato. Per risalire a questo tipo di associazione, la si può recuperare all'interno del file `phygital.routing.yml`. Il nome interno è definito nella prima riga di ogni "paragrafo", mentre il percorso web relativo è all'interno del campo "path".

Esiste inoltre un file chiamato `phygital.libraries.yml`, che permette di associare a ognuna delle pagine identificabili tramite metodo suddetto delle librerie esterne, ovvero file CSS e Javascript ad esse associate. Questi file CSS e Javascript sono normalmente collocati all'interno delle cartelle omonime.

La cartella `templates` contiene i template TWIG che costruiscono l'ossatura HTML per ogni singola pagina disponibile.

La pagina `/itinerario` in particolare si affida a `phygital-itinerario.html.twig`. Il file JS più importante associato a questa pagina è invece `itinerario.js`, che definisce tutte le funzioni più importanti tra cui quella per visualizzare i punti di interesse. I dettagli vengono recuperati dalla funzione `dettaglioPOI`, al cui interno effettua una chiamata API presso l'endpoint `post_poi_details`, che è modificabile. Anche le relative funzionalità che permettono il recupero di tutti i POI e delle loro tipologie, `listaPOI` e `listaTipologiePOI`, si appoggiano a ben definiti endpoint API. La lista degli itinerari viene recuperata dalla funzione `listait` tramite l'endpoint `post_itinerari_list`: la chiamata con `uid` uguale a 0 restituisce gli itinerari del Comune, mentre per l'utente loggato viene effettuata in aggiunta la chiamata con il suo `uid`, così da mostrare sia la sezione "Itinerari del Comune" sia la sezione "Itinerari dell'Utente".

La pagina `/crea-itinerario` si appoggia invece a `phygital-creaitinerario.html.twig` e `creaitinerario.js`. Alcune delle funzionalità precedenti sono anche qui utilizzate. `window.aggiungi=function` gestisce l'aggiunta dei punti, mentre i passaggi successivi vengono elaborati con `$("#prosegui").click` e `$("#salva").click`. I marcatori vengono rimossi seguendo la logica di `window.rimuoviMarker=function`. La chiamata API relativa alle sferiche è situata verso la fine del codice e si basa su dei file JSON definiti all'interno delle variabili `urlversione` e `urlsferiche`.

### Icone delle tipologie POI

Il menu "Visualizza su mappa" delle pagine `/itinerario`, `/crea-itinerario` e `/crea-itinerario-comune` elenca le tipologie restituite dall'endpoint `get_tipologie_poi` che hanno almeno un POI georeferenziato, più le voci Bagni (`999`) e Bike (`998`), i cui dati provengono dal geoserver comunale.

L'icona di ogni tipologia, usata sia nel menu sia per i marker sulla mappa, è il campo `icona` restituito dall'API. Se il campo è vuoto o non è un URL valido, oppure l'immagine non si carica, viene usata l'icona di default `css/files/poi-default.png`. Bagni e Bike usano le icone locali `css/files/999.png` e `css/files/998.png`. La logica è nella funzione `iconaTipologia` di `itinerario.js`, `creaitinerario.js` e `creaitinerario_comune.js`.

Nei file Javascript sono dichiarate le seguenti variabili:

-   `env_root` (in `creaitinerario_comune.js`, `creaitinerario.js`, `itinerario.js`, `phygital.js`), che corrisponde alla root della pagina aperta (`location.origin`) e viene usata per costruire gli URL di `fetch_api_call.php`, `ors_proxy.php` e del JSON delle sferiche
-   `dev_root` (in `creaitinerario_comune.js`, `creaitinerario.js`, `itinerario.js`), letta da `drupalSettings.phygital`, che corrisponde alla root dell'ambiente di test (es. `https://dev.phygital.bbsitalia.com`, senza slash finale). Quando `location.origin` coincide con `dev_root` viene forzato l'utente di test (`dev_user_code`, `dev_user_id`)

I file `fetch_api_call.php`, `ors_proxy.php` e `proxy_get.php` sono da spostare all'interno della root Drupal dell'applicazione principale, ovvero dentro `/web`.

### Configurazione (phygital.settings.php)

I parametri dipendenti dall'ambiente sono raccolti in un unico file, `phygital.settings.php`, da creare nella cartella del modulo copiando `phygital.settings.example.php` e compilandone i valori. Il file è escluso da git (vedi `.gitignore`).

-   `api`: parametri delle API visitgenoa, usati da `fetch_api_call.php`
-   `ors`: parametri di OpenRouteService, usati da `ors_proxy.php`
-   `db`: parametri per la tabella `ors_cache`, usati da `ors_proxy.php`
-   `js`: valori pubblici (`dev_root`, utente forzato su dev) esposti ai file Javascript come `drupalSettings.phygital`. Dopo averli modificati è necessario svuotare la cache di Drupal (`drush cr`)

`fetch_api_call.php` e `ors_proxy.php` cercano `phygital.settings.php` nella propria cartella oppure, una volta copiati in `/web`, in `/web/modules/custom/phygital`.

`fetch_api_call.php` è un proxy utilizzare per gestire tutte le chiamate API; l'endpoint da utilizzare si imposta tramite `api.base_url` in `phygital.settings.php`.

`proxy_get.php` viene utilizzato dall'app web 360 per evitare problemi di CORS.

`ors_proxy.php` serve per le operazioni di caching. Per far sì che funzioni è necessario creare una nuova tabella all'interno del database denominata `ors_cache`. Importare struttura e oggetti già cachati utilizzando il file `phygital_ors_table.sql`.

## Version History

### Versione 1.6

-   Le icone delle tipologie POI (menu "Visualizza su mappa" e marker sulla mappa) sono lette dal campo `icona` dell'endpoint `get_tipologie_poi` invece che da file locali
-   Aggiunta l'icona di default `css/files/poi-default.png`, usata quando il campo `icona` è vuoto o non valido oppure quando l'immagine remota non si carica
-   Da utente loggato la pagina `/itinerario` mostra anche gli Itinerari del Comune (chiamata aggiuntiva a `post_itinerari_list` con `uid` 0)

### Versione 1.5

-   Introdotto il file di configurazione `phygital.settings.php` (da creare a partire da `phygital.settings.example.php`) che raccoglie in un unico punto i parametri dipendenti dall'ambiente
-   `fetch_api_call.php` e `ors_proxy.php` leggono i parametri dal file di configurazione, sia dalla cartella del modulo sia una volta copiati in `/web`
-   I file Javascript ricevono `dev_root` e l'utente di test tramite `drupalSettings.phygital`
-   Il recupero dei POI in `phygital.js` passa ora dal proxy `fetch_api_call.php`, come già avviene in `itinerario.js`

### Versione 1.4

Aggiunta notifica all'utenza dell'attuale indisponibilità della funzionalità web 360 su dispositivi mobili

### Versione 1.3

Aggiunti file proxy per fetch chiamate API e gestione ORS cache

### Versione 1.2

Migliorie sull'accessibilità

### Versione 1.1

-   Aggiunti loader caricamento mappe
-   Ora compaiono gli indirizzi, il filtro dei poi funziona e il percorso viene centrato sulla mappa
