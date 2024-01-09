# Tour 360

build version 1.1.0

L'applicazione sviluppata in Unity è accessibile direttamente dal browser grazie all'esportazione in WebGL, offrendo un'esperienza fluida e informativa per l'utente.<br>

Inizialmente, l'applicazione legge le informazioni dell'utente e l'itinerario precedentemente selezionato dal cookie del sito ospitante. Il parametro linguistico, essenziale per personalizzare l'esperienza, viene estratto dall'URL della pagina che ospita l'applicazione.<br>

Utilizzando queste informazioni come punto di partenza, l'applicazione effettua una chiamata API per recuperare un file JSON contenente la lista delle coordinate sferiche che compongono il percorso, precedentemente acquisite dal cookie. Tali coordinate vengono quindi utilizzate per creare un percorso immersivo di sferiche, rappresentando così il tragitto dell'utente.<br>

Successivamente, l'applicazione richiama il portale di visita per ottenere informazioni cruciali, come le categorie dei punti di interesse turistici. Sfruttando queste categorie, vengono effettuate ulteriori chiamate API per individuare e istanziare i punti di interesse, sia turistici che storici, presenti nel percorso.<br>

Tutti questi punti di interesse, unitamente alle sferiche geolocalizzate che rappresentano il tragitto, vengono posizionati su un piano sfruttando la proiezione geografica WGS84. Le foto a 360 gradi del percorso vengono inizialmente caricate a bassa risoluzione per garantire un caricamento rapido, seguite successivamente dalle versioni ad alta definizione.<br>

Al fine di garantire un'esperienza efficiente, è stato implementato un sistema di buffering dinamico per il caricamento progressivo delle foto a 360 gradi, consentendo un accesso rapido senza la necessità di caricare tutto inizialmente.<br>

Durante il tour, l'utente può esplorare i vari punti di interesse cliccando su prefab istanziati sul suolo oppure avviare il tour in modalità automatica.<br>

Inoltre, è stata integrata una mappa interattiva posizionata in basso a sinistra, utilizzando il plug-in Unity Online Maps della Infinity Code. Questa mappa consente all'utente di orientarsi e comprendere la propria posizione durante l'esplorazione del tragitto.<br>

L'utente ha la possibilità di accedere a informazioni dettagliate sui punti di interesse o ascoltare un'audioguida rappresentata dai modelli 3D degli avatar di Paganini e della duchessa di Galliera. La modellazione degli avatar è stata realizzata attraverso i software Blender e Maya3D, con il texturing e i materiali creati utilizzando il plug-in Flexible Cell Shader.

## WebGL

> La build WebGL in Unity rappresenta il processo di conversione di un progetto Unity in un'applicazione web che può essere >eseguita direttamente nel browser. Questo consente agli sviluppatori di distribuire il proprio gioco o applicazione su >piattaforme web senza richiedere alcun plugin o installazione aggiuntiva.

* Compilazione del Progetto Unity: Inizialmente, il progetto Unity viene compilato per generare il codice sorgente in C# e i file asset necessari. Unity converte quindi questo codice in WebAssembly (WASM), un formato binario eseguibile nel browser.
* Creazione di File HTML, JavaScript e Asset: Durante la compilazione, Unity genera file HTML e JavaScript che fungono da interfaccia tra il browser e il codice WebAssembly. Vengono anche generati file asset come modelli 3D, texture e audio necessari per l'applicazione.
* Ottimizzazione e Compressione: I file risultanti, inclusi il codice WebAssembly, gli asset e gli script JavaScript, vengono ottimizzati e compressi per garantire un caricamento rapido e un'efficienza delle risorse durante l'esecuzione nel browser.
* Generazione della Struttura delle Directory: Unity crea una struttura di directory che contiene tutti i file necessari, organizzati in modo che l'applicazione possa essere eseguita correttamente nel browser.
* Distribuzione e Hosting: Una volta generati i file, l'applicazione può essere distribuita su un server

## Struttura

> Questo software permette la fruizione di tour creati dal portale visit, attravserso un database di foto360 caricate presso >il servizio AWS.<br>

La lettura di questo documento è atta a comprendre tutti i passaggi che avvengono nel processo di creazione >dell'itinerario.<br>

Tutti gli elementi cruciali sono integrati in un'unica scena Unity.<br>

I passaggi chiave del progetto sono:

* Lettura delle informazioni dell'utente e dell'itinerario dal cookie del browser
* Lettura della parametro della lingua nell'url della pagina
* Lettura della versione del file Json delle sferiche
* Lettura del file Json delle sferiche
* Salvataggio del file Json con relativa versione in locale
* Costruzione della polyline e calcolo dei waypoint
* Costruzione del percorso tra i WayPoint e le sferiche disponibili attraverso il Json delle sferiche
* Caricamento della categoria dei POI con relative icone
* Caricamento dei POI turistici
* Caricamento dei negozi
* Avvio del tour

## Features

* Proxy
* Caching

> La nostra app fa uso di un proxy per migliorare l'efficienza e la sicurezza delle chiamate alle API. Quando fai una richiesta API attraverso la nostra app, anziché comunicare direttamente con il server di destinazione, la tua richiesta passa attraverso il nostro server proxy. Questo server proxy, situato tra la tua app e il server API, elabora la richiesta per conto tuo.<br>

Le principali ragioni per cui utilizziamo un proxy sono:
Sicurezza: Il proxy può agire da scudo per il tuo dispositivo, nascondendo la tua identità e la tua posizione. Questo contribuisce a proteggere la tua privacy e a prevenire accessi non autorizzati.

> Ottimizzazione delle prestazioni: Il proxy può memorizzare in cache le risposte delle richieste precedenti. Quando una richiesta identica viene effettuata, il proxy può fornire la risposta dalla cache, accelerando i tempi di risposta.

> Nel caso si volesse modificare l'URL o gli endpoint del proxy basta accedere allo script

```
public class WebProxy
```

e modificare i valori

```
 private const string proxyPageApi = <span class="hljs-string">"fetch_api_call.php"</span>;
 private const string proxyPageGet = <span class="hljs-string">"proxy_get.php"</span>;
```

* BlackList

> Abbiamo introdotto un sistema di blacklist necessario per escludere le sferiche che devono essere escluse dal tour.

* Lettura dei file CSV:<br>

Il sistema inizia leggendo i due file CSV contenenti le liste di coordinate associate ai file name.
* Elaborazione del primo CSV:<br>

Il sistema estrae le coordinate e i relativi file name dal primo CSV e crea una struttura dati (ad esempio, una lista di dizionari) per rappresentare queste informazioni.
* Lettura del secondo CSV:<br>

Il sistema legge il secondo CSV contenente solo i file name che devono essere esclusi.
* Identificazione dei file da escludere:<br>

Il sistema identifica i file name presenti nel secondo CSV come quelli che devono essere esclusi dalla lista delle coordinate.
* Generazione del JSON:<br>

Il sistema crea un nuovo JSON includendo solo le coordinate e i relativi file name che non sono stati esclusi in base alla lista ottenuta nel passo precedente.
* Output del JSON:<br>

Il sistema genera un file JSON contenente la lista scremata di coordinate con i relativi file name e lo rende disponibile per l'uso successivo.
* Questo sistema consente di comparare i due file CSV e generare un JSON che contiene solo le coordinate e i file name che non devono essere esclusi, in base ai file name presenti nel secondo CSV.

> La App360 legge direttamente il file Json nello script

```
public class JsonReader : MonoBehaviour

private string sfericheVersioneEndpoint = BaseURL + <span class="hljs-string">"/sites/default/files/sferiche/versionesferiche.json"</span>;
private string sfericheEndpoint = BaseURL + <span class="hljs-string">"/sites/default/files/sferiche/sferiche.json"</span>;
```

## Packages

Per lo sviluppo dell'applicativo abbiamo fatto uso del pacchetto offerto da Infinity Code: Online Maps<br>

Online Maps è una soluzione multipiattaforma universale per la creazione di mappe 2D e 3D in Unity.<br>

Completamente personalizzabile, incredibilmente facile da apprendere e utilizzare e allo stesso tempo è una delle soluzioni più potenti e flessibili del settore.<br>

Supporta un numero enorme di servizi per qualsiasi esigenza e si integra con le migliori risorse di Asset Store.

* [[https://infinity-code.com/assets/online-maps](https://infinity-code.com/assets/online-maps)] - Infinity Code Online Maps!

### Lettura delle informazioni dell'utente e dell'itinerario dal cookie del browser

Questa parte è accessibile nello script:

```
JSInterop.js
```

### Lettura della parametro della lingua nell'url della pagina

Questa parte è accessibile nello script:

```
public class JSInterop : MonoBehaviour
```

nel metodo

```
public void Itinerario(string json)
```

### Lettura della versione del file Json e del file sferiche

Questa parte è accessibile nello script:

```
public class JsonReader : MonoBehaviour
```

gli endpoint di riferimento per la lettura della versione delle sferiche e delle sferiche sono in queste variabili stringa

```
private string sfericheVersioneEndpoint = BaseUrl+<span class="hljs-string">"/sites/default/files/sferiche/versionesferiche.json"</span>;
private string sfericheEndpoint = BaseUrl+<span class="hljs-string">"/sites/default/files/sferiche/sferiche.json"</span>;
```

### Salvataggio del file Json con relativa versione in locale

Questa parte è accessibile nello script:

```
public class JsonReader : MonoBehaviour
```

nel metodo

```
  void SaveSferiche()
```

In questo metodo viene chiamato il metodo SyncFiles() definito in un'interfaccia JavaScript JSInterop.js, che sincronizza i file.

### Costruzione della polyline e calcolo dei waypoint

Questa parte essenziale della applicazione prevede la costruzione della polilinea sulla mappa dal provider OpenStreetMap e l'istanzamento dei marker che rappresentano le sferiche trovate comprarando i waypoint e le coordinate del json.

```
public class CtrlCoordinate : MonoBehaviour
```

nel metodo

```
  private void InizializzaPercorso(ItinerarioJS itinerario)
```

> Questo script, fa quanto segue:

Preliminari:

Carica una lista di coordinate (route) che rappresentano un percorso.<br>

Imposta alcune variabili di distanza massima (maxWaypointDistance).

> Questa variabile determina la massima distanza tra i waypoint entro cui cercare le sferiche.

Ricerca delle Sferiche:

Per ogni punto nel percorso (route), cerca le sferiche più vicine.<br>

La ricerca è limitata a una certa distanza (areaDist), e se una sferica è troppo vicina, viene ignorata (minDist).<br>

Memorizza le sferiche trovate in una lista (routeSferiche), evitando duplicati.<br>

In sostanza, il codice determina quali sferiche sono vicine a ciascun punto del percorso e le salva in una lista separata per l'elaborazione successiva.

> areaDist: Questa variabile rappresenta il raggio entro cui cercare le sferiche attorno a ciascun punto nel percorso.<br>

breakDist:Questa variabile determina una distanza di "interruzione" durante la ricerca delle sferiche. Se la distanza tra un punto del percorso e una sferica è minore di breakDist, la ricerca si interrompe per quel punto.

### Caricamento della categoria dei POI con relative icone

Questa parte è accessibile nello script:

```
public class GetTipologiePOI : MonoBehaviour
```

L'obiettivo principale di questo script è scaricare e visualizzare le tipologie dei POI nell'interfaccia utente, consentendo loro di essere selezionate e utilizzate nel contesto dell'applicazione.

### Caricamento dei POI turistici

Questa parte è accessibile nello script:

```
public class GETPointOfInterest : MonoBehaviour
```

L'obiettivo principale di questo script è scaricare tutte le informazioni relative ai POI dal portale Drupal ed istanziare tutti i gameObject geolocalizzati attraverso la funzione GeoToUtm con il sistema di proiezione cartografico Wgs84, e gestire le interazioni lato utente riguardante la visualizzazione del dettaglio del singolo POI.

Gli endpoit di riferimento per le API sono i seguenti

```
static string poiDetailsEndpoint = <span class="hljs-string">"/post_poi_details"</span>;
static string poiListEndpoint = <span class="hljs-string">"/get_poi_list"</span>;
static string pointsURL = <span class="hljs-string">"/user/add_points"</span>;
```

### Caricamento dei negozi

Questa parte è accessibile nello script:

```
public class GETShops : MonoBehaviour
```

L'obiettivo principale di questo script è scaricare tutte le informazioni relative ai negozi dal portale Drupal ed istanziare tutti i gameObject geolocalizzati attraverso la funzione GeoToUtm con il sistema di proiezione cartografico Wgs84, e gestire le interazioni lato utente riguardante la visualizzazione del dettaglio del singolo negozio.

Gli endpoit di riferimento per le API sono i seguenti

```
string apiURL = <span class="hljs-string">"/get_shop_list"</span>;
static string shopDeteilsEndpoint = <span class="hljs-string">"/post_shop_details"</span>;
```

### Avvio del tour

> Una volta completati tutti i processi precedenti viene avviato il tour e quindi vengono richiamati i metodi contentui all'interno dello script :

```
public class CTRL_Player : MonoBehaviour
```

> Gestisce il movimento dell'utente all'interno del tour, con particolare attenzione alle transizioni tra posizioni e alla manipolazione delle sfere presenti nel gioco. Viene utilizzato anche per controllare la trasparenza e l'opacità di alcuni materiali. All'interno del codice, sono presenti diverse variabili per regolare il movimento, la velocità, la trasparenza e altre caratteristiche. L'utente può iniziare o mettere in pausa il tour, il tutto influendo sulla sua posizione e rotazione all'interno dell'ambiente della simulazione.

slideDuration: Questa variabile controlla la durata in secondi dell'interpolazione tra le immagini durante il tour.

```
public <span class="hljs-built_in">float</span> slideDuration = <span class="hljs-number">1</span>; // Durata <span class="hljs-keyword">in</span> secondi dell<span class="hljs-string">'interpolazione
</span>
```

secondiStepMin e secondiStepMax: Queste variabili determinano il tempo minimo e massimo in secondi tra i "passi" del tour, influenzando quindi la velocità del tour.

```
public <span class="hljs-built_in">float</span> secondiStepM<span class="hljs-keyword">in</span> = <span class="hljs-number">1</span>f;
public <span class="hljs-built_in">float</span> secondiStepMax = <span class="hljs-number">10</span>f;
```

pausePlayer: Questa variabile booleana controlla se il gioco è in pausa o meno. Se è true, il gioco è in pausa.

```
public bool pausePlayer = <span class="hljs-literal">false</span>;
```

> Il caricamento delle immagini avviene tramite Amazon Web Services (AWS) utilizzando l'oggetto JsonReader. Questo oggetto gestisce la richiesta e il download delle immagini in alta risoluzione dal servizio AWS. Quando il tour inizia o si sposta a una nuova posizione, il metodo EvaluateBuffer viene invocato per richiedere ad AWS le immagini in alta risoluzione associate alla posizione corrente o successiva. Successivamente, l'immagine scaricata viene applicata ai materiali delle sfere (sferaA e sferaB) tramite i rispettivi materiali e texture. Il processo di caricamento delle immagini in alta risoluzione avviene in modo asincrono, consentendo al tour di continuare una volta completato il caricamento delle immagini.
