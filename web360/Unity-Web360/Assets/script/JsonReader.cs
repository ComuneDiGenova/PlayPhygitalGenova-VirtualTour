using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


public class JsonReader : MonoBehaviour
{
    public static event VoidEventhandler OnReady;
    //public ListaSferiche listaSferiche = new ListaSferiche();
    //public List<string> filenames = new List<string>();
    CtrlCoordinate ctrlCoordinate;
    [SerializeField] CTRL_Player cTRL_Player;

    //[SerializeField] private TextAsset jsonFile;
    private string amazonBaseUrl;
    private string lowresUrl = "ridimensionate-basse/";
    private string hiresUrl = "ridimensionate-alte/";
    //private string sfericheVersioneEndpoint = ;
    private string sfericheVersioneEndpoint;
    private string sfericheEndpoint;



    //[SerializeField] private TextAsset jsonFile;
    //public string savePath = "Assets/Images/";
    string path;
    /*
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject CTN_Loading;
    */

    [SerializeField] bool useLocalFile = false;

    [Header("Debug")]
    [SerializeField] bool instantiateDebugMarkers = false;
    [SerializeField] Texture2D iconaSferica;
    [SerializeField] float iconaScale = 0.2f;


    //sDictionary<GeoCoordinate.Coordinate,PointData> sferiche = new Dictionary<GeoCoordinate.Coordinate, PointData>();
    public List<PanoPoint> textureBuffer = new List<PanoPoint>();
    PanoPoint current;


    private int value;

    public static ListaSferiche listaSferiche = null;
    ListaSferiche listaSfericheLocale;
    ListaSferiche listaSfericheTMP;

    private void Awake()
    {
        ctrlCoordinate = gameObject.GetComponent<CtrlCoordinate>();
        //CtrlCoordinate.OnPercorso += ControlloListaCordinateSuListaSferiche;
        //CtrlCoordinate.OnPercorso += Sferiche;
        //CTN_Loading.SetActive(true);
    }

    void Start()
    {
        StartCoroutine(DownloadJSON());
    }

    IEnumerator DownloadJSON()
    {
        string localpath;
        Debug.Log("GET SFERICHE LIST LOCALE");
        if (GetSferiche() == false)
        {
            Debug.Log("Use Streaming Asset Local");
            //LOCALE
#if UNITY_EDITOR
            localpath = "file://" + Application.streamingAssetsPath + "/sferiche.json";
#else
            localpath = Application.streamingAssetsPath + "/sferiche.json";
#endif
            Debug.Log(localpath);
            using (UnityWebRequest uwr = UnityWebRequest.Get(localpath))
            {
                yield return uwr.SendWebRequest();
                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    //PRENDO LISTA SALVATA IN LOCALE
                    var jsonData = uwr.downloadHandler.text;
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        listaSfericheLocale = JsonUtility.FromJson<ListaSferiche>(jsonData);
                    }
                    else
                    {
                        listaSfericheLocale = null;
                    }
                }
                else
                {
                    Debug.LogError("Errore nella richiesta: " + uwr.error);
                    listaSfericheLocale = null;
                }
            }
        }
        else
        {
            Debug.Log("Use Saved Local");
        }
        //
        if (!useLocalFile)
        {
            Debug.Log("GET SFERICHE LIST REMOTO");
            bool wait = true;
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            WebProxy.Get(sfericheVersioneEndpoint + "?t=" + timestamp, (json) =>
            {
                //WebProxy.GetDirect(sfericheVersioneEndpoint, (json) => {
                var versione = JsonUtility.FromJson<VersioneSferiche>(json);
                Debug.Log("Remoto versione: " + versione.versione);
                float ver_loc = 0, ver_rem = 0;
                bool rem = float.TryParse(versione.versione, NumberStyles.Float, CultureInfo.InvariantCulture, out ver_rem);
                if (listaSfericheLocale != null)
                {
                    bool loc = float.TryParse(listaSfericheLocale.versione, NumberStyles.Float, CultureInfo.InvariantCulture, out ver_loc);
                }
                Debug.LogWarning($"locale: {ver_loc.ToString()} - remoto: {ver_rem.ToString()}");
                if (ver_rem > ver_loc)
                {
                    Debug.Log("Sferiche remote più aggiornate");
                    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    WebProxy.GetDirect(sfericheEndpoint + "?t=" + timestamp, (json) =>
                    {
                        listaSfericheTMP = JsonUtility.FromJson<ListaSferiche>(json);
                        wait = false;
                        Debug.Log("Sferiche remote scaricate");
                        SaveSferiche();
                    }, () =>
                    {
                        //use local
                        wait = false;
                        Debug.Log("Sferiche da file locale");
                    });
                }
                else
                {
                    Debug.Log("Uso Sferiche locali aggiornate");
                    listaSfericheTMP = listaSfericheLocale;
                    wait = false;
                }
            }, () =>
            {
                //use local
                wait = false;
                listaSfericheTMP = listaSfericheLocale;
                Debug.Log("Sferiche da file locale");
            });
            Debug.Log("Wait for Poi List");
            yield return new WaitWhile(() => wait);
        }
        else
        {
            listaSfericheTMP = listaSfericheLocale;
        }
        if (listaSfericheTMP == null)
        {
            Debug.LogError("NO SFERICHE");
        }
        else
        {
            listaSferiche = listaSfericheTMP;
            Debug.LogWarning("Lista Sferiche: " + listaSferiche.sferiche.Count);
            if (instantiateDebugMarkers)
                InstantiateMarkers();
        }
    }

    void SaveSferiche()
    {
        string path = Application.persistentDataPath + "/sferiche.json";
        Debug.Log("Write to: " + path);
        File.WriteAllText(path, JsonUtility.ToJson(listaSfericheTMP));
        PlayerPrefs.SetString("versione", listaSfericheTMP.versione);
        PlayerPrefs.Save();
#if UNITY_WEBGL && !UNITY_EDITOR
        JSInterop.SyncFiles();
#endif

    }
    bool GetSferiche()
    {
        string path = Application.persistentDataPath + "/sferiche.json";
        Debug.Log("Read from: " + path);
        /*
        if(File.Exists(path)){
            string json = File.ReadAllText(path);
            listaSfericheLocale = JsonUtility.FromJson<ListaSferiche>(json);
            return listaSfericheLocale.versione != null;
        }else{
            return false;
        }*/
        try
        {
            string json = File.ReadAllText(path);
            listaSfericheLocale = JsonUtility.FromJson<ListaSferiche>(json);
            return listaSfericheLocale.versione != null;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }

    }

    void InstantiateMarkers()
    {
        foreach (var s in listaSferiche.sferiche)
        {
            var m = OnlineMapsMarkerManager.CreateItem(s.ToCoordinate().ToVector2(), iconaSferica, "sferica");
            m.scale = iconaScale;
        }
    }

    /*
        void Sferiche(){
            Debug.Log("sferiche");
            //StartCoroutine(ElaboraSferiche());
            OnReady.Invoke();
        }
    */
    /*
        IEnumerator ElaboraSferiche(){
            if(listaSferiche != null){

                if(listaSferiche.sferiche.Count == 0) yield break;
                //
                Debug.Log("coordinateList:" + CtrlCoordinate.coordinateList.Count);
                sferiche = GetPointDataFromCoordinates(CtrlCoordinate.coordinateList, listaSferiche);

                Debug.Log("sferiche: " + sferiche.Count);
                if(sferiche.Count == 0) yield break;

                slider.maxValue = sferiche.Count;

                ///StartCoroutine(Immagini360daServer(sferiche)); // no va in parallelo
                yield return Immagini360daServer(sferiche); //aspetta che abbia finito

                // Ora puoi fare quello che desideri con i dati JSON, ad esempio analizzarli
                // Utilizza la libreria JSONUtility o Newtonsoft.Json per analizzare il file JSON.
                CTN_Loading.SetActive(false);
                OnReady.Invoke();
            }
        }

    */
    /*
        public void ControlloListaCordinateSuListaSferiche()
        {
            Debug.LogWarning("Richiamato dall'evento statico");
            StartCoroutine(DownloadJSON());

        }
    */

    //prendo i nomi degli url in base alla comparazione della liusta di punti

    private Dictionary<GeoCoordinate.Coordinate, PointData> GetPointDataFromCoordinates(List<GeoCoordinate.Coordinate> coordinatesList, ListaSferiche listaSferiche)
    {
        //Debug.unityLogger.logEnabled = false;


        Dictionary<GeoCoordinate.Coordinate, PointData> points = new Dictionary<GeoCoordinate.Coordinate, PointData>();

        for (int i = 0; i < coordinatesList.Count; i++)
        {
            GeoCoordinate.Coordinate coordinate = coordinatesList[i];
            foreach (PointData point in listaSferiche.sferiche)
            {

                if (coordinate.Longitude == point.X && coordinate.Latitude == point.Y)
                {
                    //filenames.Add(point.Filename);
                    if (!points.ContainsKey(coordinate))
                        points.Add(coordinate, point);
                    break; // Esci dal loop interno quando trovi una corrispondenza
                }
            }
        }

        //Debug.unityLogger.logEnabled = true;
        return points;
    }

    /*************** BUFFER ***********************/

    //texture buffer da 3 elementi!

    public void EvaluateBuffer(PanoPoint currentLocation, Action callback)
    {
        Debug.Log("Eval buffer:" + currentLocation.ToString() + " current : " + current);
        //if(currentLocation == current) return;
        current = currentLocation;
        //valuta index in buffer
        if (textureBuffer.Count == 0)
            StartCoroutine(ResetBuffer(currentLocation, callback));
        else
        {
            if (textureBuffer.Contains(currentLocation))
            {
                int i = textureBuffer.IndexOf(currentLocation);
                // se 3 togli primo ed aggiungi dietro
                if (i == 2)
                    StartCoroutine(LastInBuffer(currentLocation, callback));
                //se 1 e non inizio togli ultimo e aggiungi adanti
                if (i == 0)
                    StartCoroutine(FirstInBuffer(currentLocation, callback));
                //se 2 non fare nulla
                if (i == 1)
                    StartCoroutine(MiddleInBuffer(currentLocation, callback));
            }
            else
            {
                // fuori resetta
                StartCoroutine(OutOfBuffer(currentLocation, callback));
            }
        }
    }

    IEnumerator FirstInBuffer(PanoPoint pano, Action callback = null)
    {
        Debug.Log("First in Buffer");
        int index = CtrlCoordinate.PanoTour.IndexOf(pano);
        if (index <= 0) yield break;
        if (textureBuffer[2] != null)
        {
            if (textureBuffer[2].lowResTexture != null)
                Destroy(textureBuffer[2].lowResTexture);
            if (textureBuffer[2].hiResTexure != null)
                Destroy(textureBuffer[2].hiResTexure);
            textureBuffer[2].lowResTexture = null;
            textureBuffer[2].hiResTexure = null;
        }
        textureBuffer.RemoveAt(2);
        var pt = CtrlCoordinate.PanoTour[index - 1];
        yield return Immagine360(pt.pointData, (texture) =>
        {
            pt.lowResTexture = texture;
            callback?.Invoke();
        });
        textureBuffer.Insert(0, pt);
        //callback?.Invoke();
        yield return true;
    }

    IEnumerator MiddleInBuffer(PanoPoint pano, Action callback = null)
    {
        Debug.Log("Middle Buffer");
        int index = CtrlCoordinate.PanoTour.IndexOf(pano);
        int txts = 0;
        if (textureBuffer[2] == null && index < CtrlCoordinate.PanoTour.Count - 1)
        {
            var pt = CtrlCoordinate.PanoTour[index + 1];
            yield return Immagine360(pt.pointData, (texture) =>
            {
                pt.lowResTexture = texture;
                txts++;
            });
            textureBuffer.Add(pt);
        }
        if (textureBuffer[0] == null && index > 1)
        {
            var pt = CtrlCoordinate.PanoTour[index - 1];
            yield return Immagine360(pt.pointData, (texture) =>
            {
                pt.lowResTexture = texture;
                txts++;
            });
            textureBuffer.Add(pt);
        }
        yield return new WaitUntil(() => txts == 2);
        callback?.Invoke();
        //yield return true;
    }

    IEnumerator LastInBuffer(PanoPoint pano, Action callback = null)
    {
        Debug.Log("Last in Buffer");
        int index = CtrlCoordinate.PanoTour.IndexOf(pano);
        if (index >= CtrlCoordinate.PanoTour.Count - 1) yield break;
        if (textureBuffer[0] != null)
        {
            if (textureBuffer[0].lowResTexture != null)
                Destroy(textureBuffer[0].lowResTexture);
            if (textureBuffer[0].hiResTexure != null)
                Destroy(textureBuffer[0].hiResTexure);
            textureBuffer[0].lowResTexture = null;
            textureBuffer[0].hiResTexure = null;
        }
        textureBuffer.RemoveAt(0);
        var pt = CtrlCoordinate.PanoTour[index + 1];
        yield return Immagine360(pt.pointData, (texture) =>
        {
            pt.lowResTexture = texture;
            callback?.Invoke();
        });
        textureBuffer.Add(pt);
        //callback?.Invoke();
        yield return true;
    }

    IEnumerator ResetBuffer(PanoPoint pano, Action callback = null)
    {
        Debug.Log("Reset Buffer");
        int index = CtrlCoordinate.PanoTour.IndexOf(pano);
        foreach (var t in textureBuffer)
        {
            if (t == null) continue;
            if (t.lowResTexture != null)
                Destroy(t.lowResTexture);
            if (t.hiResTexure != null)
                Destroy(t.hiResTexure);
            t.lowResTexture = null;
            t.hiResTexure = null;
        }
        textureBuffer.Clear();
        //
        int txts = 0;
        for (int i = -1; i < 2; i++)
        {
            int ii = i + index;
            if (ii >= 0 && ii < CtrlCoordinate.PanoTour.Count)
            {
                var pt = CtrlCoordinate.PanoTour[ii];
                yield return Immagine360(pt.pointData, (texture) =>
                {
                    pt.lowResTexture = texture;
                    txts++;
                    Debug.Log(txts);
                });
                textureBuffer.Add(pt);
            }
            else
            {
                textureBuffer.Add(null);
            }
        }
        yield return new WaitUntil(() => txts >= 2);
        callback?.Invoke();
    }
    IEnumerator OutOfBuffer(PanoPoint pano, Action callback = null)
    {
        Debug.Log("OutOf Buffer");
        int txts = 0;
        int index = CtrlCoordinate.PanoTour.IndexOf(pano);
        //mid lo sposto al primo e rimuovo
        if (textureBuffer[0] != null)
        {
            if (textureBuffer[0].lowResTexture != null)
                Destroy(textureBuffer[0].lowResTexture);
            if (textureBuffer[0].hiResTexure != null)
                Destroy(textureBuffer[0].hiResTexure);
            textureBuffer[0].lowResTexture = null;
            textureBuffer[0].hiResTexure = null;
        }
        textureBuffer[0] = textureBuffer[1];
        //assegno mid
        yield return Immagine360(pano.pointData, (texture) =>
        {
            pano.lowResTexture = texture;
            txts++;
        });
        textureBuffer[1] = pano;
        //cambio last
        if (textureBuffer[2] != null)
        {
            if (textureBuffer[2].lowResTexture != null)
                Destroy(textureBuffer[2].lowResTexture);
            if (textureBuffer[2].hiResTexure != null)
                Destroy(textureBuffer[2].hiResTexure);
            textureBuffer[2].lowResTexture = null;
            textureBuffer[2].hiResTexure = null;
        }
        //se non ho saltato all'ultimo
        if (index + 1 < CtrlCoordinate.PanoTour.Count)
        {
            var pt = CtrlCoordinate.PanoTour[index + 1];
            yield return Immagine360(pt.pointData, (texture) =>
            {
                pt.lowResTexture = texture;
                txts++;
            });
            textureBuffer[2] = pt;
        }
        else
        {
            //se ho saltato all'ultimo
            textureBuffer[2] = pano;
            txts++;
        }
        //
        yield return new WaitUntil(() => txts == 2);
        callback?.Invoke();
    }

    /********************************************************************************************************/
    /*
        IEnumerator Immagini360daServer(Dictionary<GeoCoordinate.Coordinate,PointData> sferiche)
        {
            Debug.Log("immagini 360 da server - " + sferiche.Count);
            foreach(var kvp in sferiche){

                string url = amazonBaseUrl + lowresUrl +  kvp.Value.Filename;
                Debug.Log(url);

    #if UNITY_EDITOR
                yield return GETImage(url, texture =>
                {
                    //Debug.Log($"La callback immagini 360 funziona");
                    if (texture != null)
                    {
                        value++;
                        //slider.value = value;
                        CtrlCoordinate.textures360.Add(kvp.Key,texture);
                        Debug.Log(texture.width);
                    }
                });
    #else
                bool ended = false;
                WebProxy.GETImage(url,texture =>
                {
                    //Debug.Log($"La callback immagini 360 funziona");
                    value++;
                    slider.value = value;
                    ctrlCoordinate.textures360.Add(kvp.Key,texture);
                    Debug.Log(texture.width);
                    ended = true;
                },()=>{
                    ended = true;
                });
                yield return new WaitUntil(()=>ended);
    #endif
            } 
        }
    */

    IEnumerator Immagine360(PointData sferica, Action<Texture2D> callBack)
    {
        string url = amazonBaseUrl + lowresUrl + sferica.Filename;
        Debug.Log("GetImageEditor LOWRES: " + url);
        /*
#if UNITY_EDITOR
        yield return GETImage(url, texture =>
#else*/
        yield return true;
        WebProxy.GETImage(url, texture =>
        //#endif
        {
            //Debug.Log($"La callback immagini 360 funziona");
            /*
            if (texture != null)
            {
                callBack.Invoke(texture);
                //Debug.Log(texture.width);
            }*/
            callBack.Invoke(texture);
        });
    }

    public void GetImmagine360HiRes(PointData sferica, Action<Texture2D, PointData> callBack)
    {
        StartCoroutine(Immagine360HiRes(sferica, callBack));
    }
    public void GetImmagine360HiRes(PanoPoint pano, Action<Texture2D, PanoPoint> callBack)
    {
        StartCoroutine(Immagine360HiRes(pano.pointData, (texture, sferica) =>
        {
            /*
            if (texture != null) {
                pano.hiResTexure = texture;
                callBack?.Invoke(texture);
            }
            else
            {
                callBack?.Invoke(null);
            }*/

            pano.hiResTexure = texture;
            callBack?.Invoke(texture, pano);

        }
        ));
    }

    IEnumerator Immagine360HiRes(PointData sferica, Action<Texture2D, PointData> callBack)
    {
        string url = amazonBaseUrl + hiresUrl + sferica.Filename;
        Debug.Log("GetImageEditor HIRES: " + url);
        /*
#if UNITY_EDITOR
        yield return GETImage(url, texture =>
#else*/
        yield return true;
        WebProxy.GETImage(url, texture =>
        //#endif
        {
            // Controlla se l'immagine è stata caricata correttamente
            if (texture != null)
            {
                callBack.Invoke(texture, sferica);
            }
            else
            {
                Debug.LogWarning("Immagine ad alta risoluzione non trovata");
                /*
                                // Trova l'indice dell'elemento corrente in CtrlCoordinate.PanoTour
                                int currentIndex = CtrlCoordinate.PanoTour.IndexOf(current);

                                // Controlla se ci sono ancora elementi successivi
                                if (currentIndex + 1 < CtrlCoordinate.PanoTour.Count)
                                {
                                    // Passa all'elemento successivo e carica l'immagine ad alta risoluzione
                                    PanoPoint nextPano = CtrlCoordinate.PanoTour[currentIndex + 1];
                                    cTRL_Player.canJump = true;
                                    GetImmagine360HiRes(nextPano, callBack);
                                }
                                else
                                {
                                    Debug.LogError("Nessun elemento successivo da caricare.");
                                    callBack.Invoke(null);
                                }*/
                callBack.Invoke(null, sferica);
            }
        });
    }

    IEnumerator GETImage(string url, Action<Texture2D> callback)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO /////////////////////////////////////
            //request.certificateHandler = new AcceptAnyCertificate();
            /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO /////////////////////////////////////

            // Chiamata per l'autorizzazione dell'header
            // AuthorizationAPI.AddAuthRequestHeader(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"Error: {url} -> {request.responseCode} - {request.error}");
                callback.Invoke(null);
            }
            else
            {
                //Debug.Log($"{url} : {request.downloadedBytes} bytes");
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false, false);
                texture.filterMode = FilterMode.Bilinear;
                texture.name = System.IO.Path.GetFileNameWithoutExtension(url);
                try
                {
                    texture.LoadImage(request.downloadHandler.data);
                    callback.Invoke(texture);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    callback.Invoke(null);
                }
            }
        }
    }


}