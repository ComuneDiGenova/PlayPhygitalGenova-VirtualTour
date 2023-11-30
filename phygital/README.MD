## Descrizione del modulo custom/phygital

Ogni pagina esposta tramite percorso web ha un nome interno definito all'interno di questo modulo personalizzato. Per risalire a questo tipo di associazione, la si può recuperare all'interno del file phygital.routing.yml. Il nome interno è definito nella prima riga di ogni "paragrafo", mentre il percorso web relativo è all'interno del campo "path".

Esiste inoltre un file chiamato phygital.libraries.yml, che permette di associare a ognuna delle pagine identificabili tramite metodo suddetto delle librerie esterne, ovvero file CSS e Javascript ad esse associate. Questi file CSS e Javascript sono normalmente collocati all'interno delle cartelle omonime.

La cartella templates contiene i template TWIG che costruiscono l'ossatura HTML per ogni singola pagina disponibile.

La pagina /itinerario in particolare si affida a phygital-itinerario.html.twig. Il file JS più importante associato a questa pagina è invece itinerario.js, che definisce tutte le funzioni più importanti tra cui quella per visualizzare i punti di interesse. I dettagli vengono recuperati dalla funzione dettaglioPOI, al cui interno effettua una chiamata API presso l'endpoint post_poi_details, che è modificabile. Anche le relative funzionalità che permettono il recupero di tutti i POI e delle loro tipologie, listaPOI e listaTipologiePOI, si appoggiano a ben definiti endpoint API.

La pagina /crea-itinerario si appoggia invece a phygital-creaitinerario.html.twig e creaitinerario.js. Alcune delle funzionalità precedenti sono anche qui utilizzate. window.aggiungi=function gestisce l'aggiunta dei punti, mentre i passaggi successivi vengono elaborati con $("#prosegui").click e $("#salva").click. I marcatori vengono rimossi seguendo la logica di window.rimuoviMarker=function. La chiamata API relativa alle sferiche è situata verso la fine del codice e si basa su dei file JSON definiti all'interno delle variabili urlversione e urlsferiche.

Nei file Javascript creaitinerario_comune.js, creaitinerario.js, itinerario.js, phygital.js, sono dichiarate le seguenti variabili:

-   dev_root, che corrisponde alla root relativa all'ambiente di test (es. https://dev.phygital.bbsitalia.com)
-   prod_root, che corrisponde alla root relativa all'ambiente di produzione (es. https://new.visitgenoa.it)
-   env_root, che corrisponde alla root attualmente utilizzata dall'applicazione. Di default corrisponde alla root della pagina che risulta essere aperta, ma se necessario la si può mettere uguale a dev_root oppure prod_root (qualora l'endpoint non corrisponda all'ambiente in uso)

I file fetch_api_call.php, ors_proxy.php e proxy_get.php sono da spostare all'interno della root Drupal dell'applicazione principale, ovvero dentro /web.

fetch_api_call.php è un proxy utilizzare per gestire tutte le chiamate API, ed è necessario che la variabile $base_url venga aggiornata di conseguenza, in base all'endpoint che si desidera utilizzare.

proxy_get.php viene utilizzato dall'app web 360 per evitare problemi di CORS.

ors_proxy.php serve per le operazioni di caching. Per far sì che funzioni è necessario creare una nuova tabella all'interno del database denominata ors_cache. Importare struttura e oggetti già cachati utilizzando il file phygital_ors_table.sql.

## Version History

## Versione 1.1

Aggiunti loader caricamento mappe
Ora compaiono gli indirizzi, il filtro dei poi funziona e il percorso viene centrato sulla mappa

## Versione 1.2

Migliorie sull'accessibilità

## Versione 1.3

Aggiunti file proxy per fetch chiamate API e gestione ORS cache
