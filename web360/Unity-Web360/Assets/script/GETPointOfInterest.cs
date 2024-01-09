using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GETPointOfInterest : MonoBehaviour
{
    public GameObject poiPrefab;
    [SerializeField] Transform poiParent;
    public ToggleCinemachineInput toggleinputCamera;
    public CinemachineInputProvider inputProvider;
    UnityEvent<float> onValueChanged;
    public GameObject popUpPunti;
    public static bool PoiIstanziati = false;
    //Contenuti dell'INFO poi turistico 
    [Space(10)]
    [Header("Contenuti dell'INFO poi turistico")]
    [SerializeField] GameObject info_poiTuristico;
    [SerializeField] GameObject BoxBianco;
    [SerializeField] GameObject ctrl_audio;
    [SerializeField] GameObject immagine_principaleInfoT;
    [SerializeField] TMP_Text TXT_titoloInfoT;
    [SerializeField] TMP_Text TXT_descrizioneInfoT;
    [SerializeField] Button ApriIlContenuto;
    [SerializeField] Sprite placeholder;
    [SerializeField] GameObject contentDescrizione;

    // contentui del POI turistico
    [Space(10)]
    [Header("contentui del POI turistico")]
    [SerializeField] GameObject ctn_poiTuristico;
    [SerializeField] GameObject immagine_principale;
    [SerializeField] TMP_Text TXT_titolo;
    [SerializeField] TMP_Text TXT_categoria;
    [SerializeField] TMP_Text TXT_descrizione;
    [SerializeField] Button Esplora;
    [SerializeField] GameObject PrefabImageGallery;
    [SerializeField] GameObject ParentGallery;

    //contentui del poi interno
    [Space(10)]
    [Header("contentui del POI turistico")]

    [SerializeField] TMP_Text TXT_titoloInt;
    [SerializeField] TMP_Text TXT_categoriaInt;
    [SerializeField] TMP_Text TXT_descrizioneInt;
    [SerializeField] GameObject PrefabButtonInsidePoi;
    [SerializeField] GameObject ParentButtonInside;
    [SerializeField] GameObject LoaderPanel;
    [Space(10)]

    string completeURL;
    public static InformationList DownloadedInformationPois = null;
    public static Dictionary<ShortInfo, Info> DownloadedExtendedInfo = new Dictionary<ShortInfo, Info>();
    public GameObject sfera360InsidePoi;
    Material Equirectangular;

    public List<Sprite> listaIcone = new List<Sprite>();
    int iconIndex;
    private AudioClip audioClip;
    Sprite spriteImgPrincipale;

    [SerializeField] InformationList infopois; //DA RIMUOVERE !!!! 

    static string poiDetailsEndpoint = "/post_poi_details";
    static string poiListEndpoint = "/get_poi_list";
    static string pointsURL = "/user/add_points";

    public CTRL_Player ctrl_Player;
    public GameObject ctrl_mappa;
    public AudioManager audioManager;

    public GameObject AvatarPaganini;
    public GameObject AvatarDuchessa;

    public List<GameObject> ListaImmaginiGalleria = new List<GameObject>();
    public List<GameObject> ListaButtoniInsideGalleria = new List<GameObject>();
    public List<GameObject> ListaPOI = new List<GameObject>();

    public static InfoUser DownloadedUserInfo;
    [SerializeField] InfoUser userInfo; //DA RIMUOVERE !!!!
    public static AddPoint AddPoint;
    public static ResponseAddPoint RequestResponse;
    public GETUserInfo getUserInfo;
    public AddFavourite addFavourite;
    public Info lastPoi;
    public GameObject loader;

    private Vector2 pozioneDefaultContentScroll;
    public static bool dentroUnPOI = false;
    //public static FavouriteList FavouriteResponse;
    //[SerializeField] FavouriteList favourites; //DA RIMUOVERE !!!!

    //necessarie per l'audio
    float listenedMin = 0.5f;
    public float currentListening = 0;
    public bool listeningPoint = false;
    int id_attuale;
    private void Awake()
    {
        //CtrlCoordinate.OnPercorso += InstantiatePoi;
        GetTipologiePOI.OnTipologiePoiTrovate += InstantiatePoi;
    }

    private void Start()
    {
        StartCoroutine(GETInfoPois());
        Equirectangular = sfera360InsidePoi.GetComponent<Renderer>().material;
        pozioneDefaultContentScroll = contentDescrizione.transform.localPosition;
        Debug.Log("transfom testo scrollView = " + pozioneDefaultContentScroll);
    }

    /* 
    private InformationList PurgeDuplicatePois(InformationList pois){ 
        Debug.Log("RAW:" + pois.infos.Count); 
        var newpois = new InformationList(); 
        foreach(var i in pois.infos) 
        { 
            if(newpois.infos.Where(x=>x.id == i.id).Count() == 0){ 
                newpois.infos.Add(i); 
            }else{ 
                var poi = newpois.infos.Where(x=>x.id == i.id).FirstOrDefault(); 
                poi.gallery_list.Add(i.gallery); 
            } 
        } 
        Debug.Log("PURGED" + newpois.pois.Count); 
        return newpois; 
    } 
    */

    private void Update()
    {
        if (audioManager.audioSource.isPlaying && !listeningPoint)
        {
            currentListening += Time.deltaTime;
            if (currentListening / audioManager.audioSource.clip.length > listenedMin)
            {
                listeningPoint = true;
                if (GETUserInfo.DownloadedUserInfo != null && GETUserInfo.DownloadedUserInfo.codice_utente != "0")
                {
                    string ascolto = "ascolto";
                    SendRecipitInfoAssegnaPunti(id_attuale, ascolto);
                }
            }
        }

    }

    public static IEnumerator GETExtendedInfo(ShortInfo shortInfo, Action<Info> callBack)
    {
        Debug.Log(shortInfo.ToString());
        if (DownloadedExtendedInfo.ContainsKey(shortInfo))
        {
            callBack.Invoke(DownloadedExtendedInfo[shortInfo]);
            yield break;
        }
        string url = AuthorizationAPI.baseURL + "/jsonapi" + poiDetailsEndpoint;
        Debug.Log("url : " + url);
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("nid", shortInfo.id.ToString());
        //headers.Add("language", ((Languages)DropDownValue.dropdownValue).ToString().ToLower());
#if UNITY_EDITOR
        using (UnityWebRequest request = UnityWebRequest.Post(url, headers))
        {
            /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 
            request.certificateHandler = new AcceptAnyCertificate();
            /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 

            // Chiamata per l'autorizzazione dell'header 
            AuthorizationAPI.AddAuthRequestHeader(request);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error while reciving: " + request.error);
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                string json = "{\"infos\" : " + request.downloadHandler.text + "}";
                Debug.Log(json);
                var info = (JsonUtility.FromJson<InfoList>(json).infos[0]);
                //
                headers.Add("language", ((Languages)DropDownValue.dropdownValue).ToString().ToLower());
                using (UnityWebRequest request2 = UnityWebRequest.Post(url, headers))
                {
                    /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 
                    request2.certificateHandler = new AcceptAnyCertificate();
                    /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 

                    // Chiamata per l'autorizzazione dell'header 
                    AuthorizationAPI.AddAuthRequestHeader(request2);

                    yield return request2.SendWebRequest();

                    if (request2.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Error while reciving: " + request2.error);
                        Debug.Log(request2.downloadHandler.text);
                    }
                    else
                    {
                        var response2 = request2.downloadHandler.text;
                        if (response2 == "[]")
                        {
                            Debug.Log($"NO Language {((Languages)DropDownValue.dropdownValue).ToString().ToLower()} for {shortInfo.id.ToString()}");
                            DownloadedExtendedInfo.Add(shortInfo, info);
                            callBack.Invoke(info);
                            yield break;
                        }
                        string json2 = "{\"infos\" : " + response2 + "}";
                        Debug.Log(json2);
                        var infolist2 = JsonUtility.FromJson<InfoList>(json2);
                        if (infolist2 != null && infolist2.infos != null && infolist2.infos.Count == 1)
                        {
                            var info2 = infolist2.infos[0];
                            DownloadedExtendedInfo.Add(shortInfo, info2);
                            callBack.Invoke(info2);
                        }
                        else
                        {
                            DownloadedExtendedInfo.Add(shortInfo, info);
                            callBack.Invoke(info);
                        }
                    }
                }
                //
                //DownloadedExtendedInfo.Add(shortInfo, info);
                //callBack.Invoke(info);
            }
        }
#else
        yield return true;
        WebProxy.PostApi(poiDetailsEndpoint,headers,(response) => {
            string json = "{\"infos\" : " + response + "}";
            Debug.Log("dopo: " + json);
            var info = (JsonUtility.FromJson<InfoList>(json).infos[0]);
            //
            headers.Add("language", ((Languages)DropDownValue.dropdownValue).ToString().ToLower());
            WebProxy.PostApi(poiDetailsEndpoint,headers,(response2) => {
                if(response2 == "[]"){
                    Debug.LogWarning($"NO Language {((Languages)DropDownValue.dropdownValue).ToString().ToLower()} for {shortInfo.id.ToString()}");
                    DownloadedExtendedInfo.Add(shortInfo, info);
                    callBack.Invoke(info);
                    return;
                }
                string json2 = "{\"infos\" : " + response2+ "}";
                Debug.Log("dopo: " + json2);
                var infolist2 = JsonUtility.FromJson<InfoList>(json2);
                if(infolist2 != null && infolist2.infos != null && infolist2.infos.Count == 1){
                    var info2 = infolist2.infos[0];
                    DownloadedExtendedInfo.Add(shortInfo, info2);
                    callBack.Invoke(info2);
                }else{
                    DownloadedExtendedInfo.Add(shortInfo, info);
                    callBack.Invoke(info);
                }
            },null);
            //
            //DownloadedExtendedInfo.Add(shortInfo, info);
            //callBack.Invoke(info);
        },null);
#endif
    }

    void InstantiatePoi()
    {
        //yield return StartCoroutine(GETInfoPois());
        Debug.LogWarning("Istanzio POI");
        if (DownloadedInformationPois != null)
        {
            foreach (var p in DownloadedInformationPois.infos)
            {
                if (p.ToCoordinate() == null || p.tipo == "negozio")
                    continue;
                //Traduco le coordinate e istanzio i poi nel mondo 3D/////////////////////
                var coordinate = p.ToCoordinate();
                GeoCoordinate.Point posizione = GeoCoordinate.Utils.GeoToUtm(coordinate);
                var poi = Instantiate(poiPrefab, poiParent);
                poi.name = p.id;
                poi.transform.SetParent(poiParent);
                var newPosition = posizione - CtrlCoordinate.start;
                poi.transform.position = newPosition.ToVector3();
                //if(p.id== "7924")
                //{
                //    poi.transform.position = new Vector3(4, 0, 6);
                //}
                var pp = poi.AddComponent<TEST_API_POI>();
                pp.Inizialize(p);
                ListaPOI.Add(poi);
                //setto l'immagine corretta in base alla categoria//
                //GetPOIType(p.id_tipologia);

                //poi.GetComponent<Image>().sprite = listaIcone[iconIndex];
                poi.GetComponent<Image>().sprite = GetTipologiePOI.GetSprite(p.id_tipologia);
                if (iconIndex != 2)
                {
                    poi.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ApriInfoPoiTuristico(poi)));
                }
                //else poi.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ApriContenutoShop(poi)));
            }
            PoiIstanziati = true;
        }
    }

    IEnumerator GETInfoPois()
    {
        completeURL = AuthorizationAPI.baseURL + "/jsonapi" + poiListEndpoint;
#if UNITY_EDITOR
        using (UnityWebRequest request = UnityWebRequest.Get(completeURL))
        {
            /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 
            request.certificateHandler = new AcceptAnyCertificate();
            /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 

            // Chiamata per l'autorizzazione dell'header 
            AuthorizationAPI.AddAuthRequestHeader(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error while reciving: " + request.error);
            }
            else
            {
                //////////////////////////////////////// 
                //Debug.Log(request.downloadHandler.text); 
                //////////////////////////////////////// 

                string json = "{\"infos\" : " + request.downloadHandler.text + "}";
                Debug.Log(json);
                DownloadedInformationPois = JsonUtility.FromJson<InformationList>(json);
                infopois = DownloadedInformationPois;
                Debug.Log("Donwloaded infopois");
            }
        }
#else
        yield return true;
        WebProxy.GetApi(poiListEndpoint,(response) => {
            string json = "{\"infos\" : " + response + "}";
            Debug.Log(json);
            DownloadedInformationPois = (JsonUtility.FromJson<InformationList>(json));
            infopois = DownloadedInformationPois;
            Debug.Log("Donwloaded infopois");
        },null);
#endif
    }



    //public void GetPOIType(string type)
    //{
    //    switch (type)
    //    {
    //        case "133": // acquedotto storico
    //            iconIndex = 0;
    //            break;
    //        case "130": // arte e cultura
    //            iconIndex = 1;
    //            break;
    //        case "Bottega Storica": // botteghe storiche
    //            iconIndex = 2;
    //            break;
    //        case "57": // genova per i bambini
    //            iconIndex = 3;
    //            break;
    //        case "55": // genova by night
    //            iconIndex = 4;
    //            break;
    //        case "37": // luoghi da scoprire
    //            iconIndex = 5;
    //            break;
    //        case "38": // mare
    //            iconIndex = 6;
    //            break;
    //        case "185": // monumenti e luoghi sacri
    //            iconIndex = 7;
    //            break;
    //        case "187": // musei chiese e monumenti
    //            iconIndex = 7;
    //            break;
    //        case "417": // chiese e monumenti
    //            iconIndex = 7;
    //            break;
    //        case "53": // mura e forti
    //            iconIndex = 8;
    //            break;
    //        case "186": // forti
    //            iconIndex = 8;
    //            break;
    //        case "14": // musei
    //            iconIndex = 9;
    //            break;
    //        case "131": // outdoor
    //            iconIndex = 10;
    //            break;
    //        case "36": // palazzi dei Rolli
    //            iconIndex = 11;
    //            break;
    //        case "188": // palazzi dei Rolli - Patrimonio Unesco
    //            iconIndex = 11;
    //            break;
    //        case "33": // parchi ville e giardini
    //            iconIndex = 12;
    //            break;
    //        case "418": // parchi ville e orti botanici
    //            iconIndex = 12;
    //            break;
    //        case "134": // punti panoramici
    //            iconIndex = 13;
    //            break;
    //        case "15": // quartieri
    //            iconIndex = 14;
    //            break;
    //        case "54": // sport
    //            iconIndex = 15;
    //            break;
    //        case "132": // storia e tradizioni
    //            iconIndex = 16;
    //            break;
    //        case "52": // teatri
    //            iconIndex = 17;
    //            break;
    //        default:
    //            iconIndex = 17;
    //            break;
    //    }
    //}


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //---------------------------APRI PAGINA CONTENUTO----------------------------------------------------/////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    IEnumerator ApriInfoPoiTuristico(GameObject poiObject)
    {
        loader.SetActive(true);
        dentroUnPOI = true;
        // mettiamo in pausa il tour virtuale
        if (ctrl_Player.startPlayer)
        {
            ctrl_Player.PlayPauseShop();
        }
        //chiudiamo la mappa
        ctrl_mappa.GetComponent<Animator>().SetTrigger("close");

        //settiamo l'altezza del content della descrizione alla sua posozione di default
        contentDescrizione.transform.localPosition = pozioneDefaultContentScroll;

        //facciamo la chiamata per prendere il dato relativo al POI
        TEST_API_POI poiComponent = poiObject.GetComponent<TEST_API_POI>();
        //var shortInfo = poiComponent.shortInfoPoi;
        poiComponent.GetEXtendedInfo();
        yield return new WaitUntil(() => poiComponent.infoPOI != null);
        id_attuale = poiComponent.infoPOI.id;
        //se divers odistruggo
        if (poiComponent.infoPOI != lastPoi)
            DistruggiDatiCtnPoi(lastPoi);
        lastPoi = poiComponent.infoPOI;

        AddFavourite.favouriteClickedMarkerLabel = poiComponent.infoPOI.id.ToString();
        Debug.Log(AddFavourite.favouriteClickedMarkerLabel);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////7
        /////////////////////////// MODIFICHIAMO UN POI MANUALMENTE, SARA DA ELIMINARE QUESTA PARTE QUA /////////////////
        ///

        // Ora puoi accedere al componente InfoPOI per ottenere le informazioni

        // Assegna i testi
        TXT_titoloInfoT.text = poiComponent.infoPOI.nome;
        TXT_titolo.text = poiComponent.infoPOI.nome;
        TXT_titoloInt.text = poiComponent.infoPOI.nome;
        string newText = TextDecoder.DecodeText(poiComponent.infoPOI.descrizione);
        TXT_descrizioneInfoT.text = newText;
        TXT_descrizione.text = newText;
        TXT_descrizioneInt.text = newText;
        immagine_principaleInfoT.GetComponent<Image>().sprite = null;
        Debug.Log($"numero di immagini 360 = {poiComponent.infoPOI.immagini360.Count}");
        if (poiComponent.infoPOI.immagini360.Count > 0)
        {
            Esplora.interactable = true;
            Esplora.onClick.RemoveAllListeners();
            Esplora.onClick.AddListener(() => EsploraContenutoTuristico(poiObject, poiComponent.infoPOI));
            Esplora.gameObject.SetActive(true);
        }
        else
        {
            Esplora.interactable = false;
            Esplora.gameObject.SetActive(false);
        }

        info_poiTuristico.SetActive(true);
        loader.SetActive(false);
        //facciamo la chimata per cercare l'immagine di copertina
        if (!string.IsNullOrEmpty(poiComponent.infoPOI.immagine_di_copertina))
        {
            Debug.Log("immagine di copertina " + poiComponent.infoPOI.immagine_di_copertina);
            string url = poiComponent.infoPOI.immagine_di_copertina;
            if (poiComponent.infoPOI.immagine_di_copertinaTexture != null)
            {
                float altezza = poiComponent.infoPOI.immagine_di_copertinaTexture.height;
                float larghezza = poiComponent.infoPOI.immagine_di_copertinaTexture.width;
                float aspectRatio = larghezza / altezza;
                Debug.Log("aspect Ratio dell'immagine già in memoria" + aspectRatio);
                spriteImgPrincipale = Sprite.Create(poiComponent.infoPOI.immagine_di_copertinaTexture, new Rect(0, 0, poiComponent.infoPOI.immagine_di_copertinaTexture.width, poiComponent.infoPOI.immagine_di_copertinaTexture.height), Vector2.one * 0.5f);
                immagine_principaleInfoT.GetComponent<Image>().sprite = spriteImgPrincipale;
                immagine_principale.GetComponent<Image>().sprite = spriteImgPrincipale;
                immagine_principaleInfoT.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                immagine_principale.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
            }
            else
            {
#if UNITY_EDITOR
                StartCoroutine(GETImage(url, texture =>
                {
                    if (texture != null)
                    {
                        poiComponent.infoPOI.immagine_di_copertinaTexture = texture;
                        float altezza = texture.height;
                        float larghezza = texture.width;
                        float aspectRatio = larghezza / altezza;
                        Debug.Log("aspect Ratio dell'immagine scaricata da unity" + aspectRatio);
                        spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                        immagine_principaleInfoT.GetComponent<Image>().sprite = spriteImgPrincipale;
                        immagine_principale.GetComponent<Image>().sprite = spriteImgPrincipale;
                        immagine_principaleInfoT.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                        immagine_principale.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                    }
                    else
                    {
                        Debug.LogError("nessuna texture trovata");
                        immagine_principaleInfoT.GetComponent<Image>().sprite = placeholder;
                    }
                }));
#else
                yield return true;
                WebProxy.GETImage(url, texture =>

                {
                    if (texture != null)
                    {
                        poiComponent.infoPOI.immagine_di_copertinaTexture = texture;
                        float altezza = texture.height;
                        float larghezza = texture.width;
                        float aspectRatio = larghezza/altezza;
                        Debug.Log("aspect Ratio dell'immagine scaricata via proxy" + aspectRatio);
                        spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                        immagine_principaleInfoT.GetComponent<Image>().sprite = spriteImgPrincipale;
                        immagine_principale.GetComponent<Image>().sprite = spriteImgPrincipale;
                        immagine_principaleInfoT.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                        immagine_principale.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                    }
                    else Debug.LogError("nessuna texture trovata");
                });
#endif

            }
        }
        else
        {
            Debug.Log("assegno l'immagine placeHolder");
            immagine_principaleInfoT.GetComponent<Image>().sprite = placeholder;
        }

        AudioClip clip = null;
        //assegnamo un audio se esiste
        if (!string.IsNullOrEmpty(poiComponent.infoPOI.audio))
        {
            string audioUrl = poiComponent.infoPOI.audio;
            string avatar = poiComponent.infoPOI.avatar;
            Debug.Log(audioUrl);
            ctrl_audio.SetActive(true);
            if (avatar == "1")
            {
                AvatarPaganini.SetActive(true);
                AvatarDuchessa.SetActive(false);
            }
            else { AvatarDuchessa.SetActive(true); AvatarPaganini.SetActive(false); }


            yield return StartCoroutine(GetAudioFromFile(audioUrl, audioclip =>
            {
                if (audioclip != null)
                {
                    ctrl_audio.GetComponent<CTRL_AUDIO>().Play(audioClip);
                    //Debug.Log("lunghezza dell'audioclip = "+audioClip.length);
                    audioManager.slideAreaRead.value = 0;
                    audioManager.slideAreaInput.value = 0;
                    //if (GETUserInfo.DownloadedUserInfo != null && GETUserInfo.DownloadedUserInfo.codice_utente != "0")
                    //{
                    //    string ascolto = "ascolto";
                    //    SendRecipitInfoAssegnaPunti(poiComponent.infoPOI.id, ascolto);
                    //}
                }
                else Debug.LogError("Nessuna audio trovato");
            }));

            //WebProxy.GetAudio(audioUrl, (aclip) => {
            //    clip = aclip;
            //    ctrl_audio.GetComponent<AudioSource>().clip = audioClip;
            //    ctrl_audio.GetComponent<AudioSource>().Play();
            //    slider.value = 0;
            //}, () => {
            //    Debug.LogError("No AUDIOCLIP");
            //});
        }

        //yield return new WaitUntil(() => clip != null);


        addFavourite.checkFavourite();
        //ApriIlContenuto.onClick.RemoveAllListeners();
        //ApriIlContenuto.onClick.AddListener(() => ApriContenutoPoiTuristico(poiObject, poiComponent.infoPOI));

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)BoxBianco.transform);
        //LayoutRebuilder.ForceRebuildLayoutImmediate((AspectRatioFitter)immagine_principaleInfoT.GetComponent<AspectRatioFitter>);


    }

    private void ApriContenutoPoiTuristico(GameObject poiObject, Info poiInfo)
    {
        Debug.Log("Turistico");

        //assegnamo il punteggio data l'azione di aver visitato il poi
        if (GETUserInfo.DownloadedUserInfo != null && GETUserInfo.DownloadedUserInfo.codice_utente != "0")
        {
            //SendRecipitInfo(poiInfo.id);
        }

        AddFavourite.favouriteClickedMarkerLabel = poiInfo.id.ToString();
        Debug.Log(AddFavourite.favouriteClickedMarkerLabel);

        //disattiviamo i comandi della camera virtuale
        toggleinputCamera.enabled = false;
        inputProvider.enabled = false;

        //assegnamo i testi
        TXT_categoria.text = poiInfo.tipologia;
        TXT_categoriaInt.text = poiInfo.tipologia;

        ctn_poiTuristico.SetActive(true);
        info_poiTuristico.SetActive(false);

        Debug.Log("numero elementi immagini 360 " + poiInfo.immagini360.Count);
        if (poiInfo.immagini360.Count > 0)
        {
            Esplora.interactable = true;
            Esplora.onClick.RemoveAllListeners();
            Esplora.onClick.AddListener(() => EsploraContenutoTuristico(poiObject, poiInfo));
            Esplora.gameObject.SetActive(true);
        }
        else
        {
            Esplora.interactable = false;
            Esplora.gameObject.SetActive(false);
        }
        ////prendiamo le immagini della gallery del poi///////////////
        foreach (var i in ListaImmaginiGalleria)
        {
            Destroy(i);
        }
        ListaImmaginiGalleria.Clear();
        Debug.Log("numero elementi immagini gallery " + poiInfo.immagini_gallery.Count);
        if (poiInfo.immagini_gallery.Count > 0)
        {
            //Debug.Log("qua dovrebbe arrivare");
            foreach (Immagini_Gallery imageGallery in poiInfo.immagini_gallery)
            {
                if (imageGallery.texture != null)
                {
                    float altezza = imageGallery.texture.height;
                    float larghezza = imageGallery.texture.width;
                    float aspectRatio = altezza / larghezza;
                    spriteImgPrincipale = Sprite.Create(imageGallery.texture, new Rect(0, 0, imageGallery.texture.width, imageGallery.texture.height), Vector2.one * 0.5f);
                    var imgGallery = Instantiate(PrefabImageGallery, new Vector3(0, 0), Quaternion.identity);
                    imgGallery.transform.SetParent(ParentGallery.transform, false);
                    imgGallery.GetComponent<Image>().sprite = spriteImgPrincipale;
                    //imgGallery.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                    //Debug.Log(texture.width);
                    ListaImmaginiGalleria.Add(imgGallery);
                }
                else
                {
                    string url = imageGallery.immagine_gallery;
#if UNITY_EDITOR
                    StartCoroutine(GETImage(url, texture =>
                    {
                        //Debug.Log("vediamo se cicla");
                        if (texture != null)
                        {
                            imageGallery.texture = texture;
                            float altezza = texture.height;
                            float larghezza = texture.width;
                            float aspectRatio = altezza / larghezza;
                            spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                            var imgGallery = Instantiate(PrefabImageGallery, new Vector3(0, 0), Quaternion.identity);
                            imgGallery.transform.SetParent(ParentGallery.transform, false);
                            imgGallery.GetComponent<Image>().sprite = spriteImgPrincipale;
                            //imgGallery.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                            //Debug.Log(texture.width);
                            ListaImmaginiGalleria.Add(imgGallery);
                        }
                        else Debug.LogError("Nessuna texture trovata");
                    }));
#else
                    WebProxy.GETImage(url, texture =>
                    {
                        //Debug.Log("vediamo se cicla");
                        if (texture != null)
                        {
                            imageGallery.texture = texture;
                            float altezza = texture.height;
                            float larghezza = texture.width;
                            float aspectRatio = altezza / larghezza;
                            spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                            var imgGallery= Instantiate(PrefabImageGallery, new Vector3(0, 0), Quaternion.identity);
                            imgGallery.transform.SetParent(ParentGallery.transform, false);
                            imgGallery.GetComponent<Image>().sprite = spriteImgPrincipale;
                            //imgGallery.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                            //Debug.Log(texture.width);
                            ListaImmaginiGalleria.Add(imgGallery);
                        }
                        else Debug.LogError("Nessuna texture trovata");
                    });
#endif
                }
            }
        }

        ////AddToFavourite();
        addFavourite.checkFavourite();

    }

    public async void EsploraContenutoTuristico(GameObject poiObject, Info poiInfo)
    {
        Debug.Log("POI 360: " + poiInfo.immagini360.Count);
        LoaderPanel.SetActive(true);
        sfera360InsidePoi.SetActive(true);
        foreach (var b in ListaButtoniInsideGalleria)
        {
            Destroy(b);
        }
        ListaButtoniInsideGalleria.Clear();
        int images = 0;
        foreach (Immagini360 img360 in poiInfo.immagini360)
        {
            Debug.Log(img360.title);
            string url = img360.immagine360;
            GameObject button = Instantiate(PrefabButtonInsidePoi);
            button.transform.SetParent(ParentButtonInside.transform);
            button.transform.localScale = new Vector3(1, 1, 1);
            button.GetComponentInChildren<TMP_Text>().text = img360.title;

            if (img360.texture != null)
            {
                spriteImgPrincipale = Sprite.Create(img360.texture, new Rect(0, 0, img360.texture.width, img360.texture.height), Vector2.one * 0.5f);
                button.transform.GetChild(0).GetComponent<Image>().sprite = spriteImgPrincipale;
                button.transform.GetChild(0).GetComponent<AspectRatioFitter>().aspectRatio = 2.7f;
                //Debug.Log(texture.width);
                ListaButtoniInsideGalleria.Add(button);
                button.GetComponent<Button>().onClick.AddListener(() => foto360InsidePoi(button));
                foto360InsidePoi(ListaButtoniInsideGalleria[0]);
                images++;
            }
            else
            {
#if UNITY_EDITOR
                StartCoroutine(GETImage(url, texture =>
                {
                    if (texture != null)
                    {
                        img360.texture = texture;
                        //float altezza = texture.height;
                        //float larghezza = texture.width;
                        //float aspectRatio = altezza / larghezza;
                        spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                        button.transform.GetChild(0).GetComponent<Image>().sprite = spriteImgPrincipale;
                        button.transform.GetChild(0).GetComponent<AspectRatioFitter>().aspectRatio = 2.7f;
                        //Debug.Log(texture.width);
                        ListaButtoniInsideGalleria.Add(button);
                        button.GetComponent<Button>().onClick.AddListener(() => foto360InsidePoi(button));
                        foto360InsidePoi(ListaButtoniInsideGalleria[0]);
                        images++;
                    }
                }));
#else
                WebProxy.GETImage (url, texture =>
                {
                    if (texture != null)
                    {
                        img360.texture = texture;
                        //float altezza = texture.height;
                        //float larghezza = texture.width;
                        //float aspectRatio = altezza / larghezza;
                        spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                        button.transform.GetChild(0).GetComponent<Image>().sprite = spriteImgPrincipale;
                        button.transform.GetChild(0).GetComponent<AspectRatioFitter>().aspectRatio = 2.7f;
                        //Debug.Log(texture.width);
                        ListaButtoniInsideGalleria.Add(button);
                        button.GetComponent<Button>().onClick.AddListener(() => foto360InsidePoi(button));
                        foto360InsidePoi(ListaButtoniInsideGalleria[0]);
                        images++;
                    }
                });
#endif
            }
        }
        //yield return new WaitUntil(() => images == poiInfo.immagini360.Count);
        while (images != poiInfo.immagini360.Count)
        {
            await System.Threading.Tasks.Task.Yield();
        }
        Debug.Log("End 360s");
        LoaderPanel.SetActive(false);
    }

    public async void EsploraContenutoTuristico(GameObject poiObject, ShopInformaitions poiInfo)
    {
        Debug.Log("POI 360: " + poiInfo.immagini360.Count);
        LoaderPanel.SetActive(true);
        sfera360InsidePoi.SetActive(true);
        foreach (var b in ListaButtoniInsideGalleria)
        {
            Destroy(b);
        }
        ListaButtoniInsideGalleria.Clear();
        int images = 0;
        foreach (Immagini360 img360 in poiInfo.immagini360)
        {
            Debug.Log(img360.title);
            string url = img360.immagine360;
            GameObject button = Instantiate(PrefabButtonInsidePoi);
            button.transform.SetParent(ParentButtonInside.transform);
            button.transform.localScale = new Vector3(1, 1, 1);
            button.GetComponentInChildren<TMP_Text>().text = img360.title;

            if (img360.texture != null)
            {
                spriteImgPrincipale = Sprite.Create(img360.texture, new Rect(0, 0, img360.texture.width, img360.texture.height), Vector2.one * 0.5f);
                button.transform.GetChild(0).GetComponent<Image>().sprite = spriteImgPrincipale;
                button.transform.GetChild(0).GetComponent<AspectRatioFitter>().aspectRatio = 2.7f;
                //Debug.Log(texture.width);
                ListaButtoniInsideGalleria.Add(button);
                button.GetComponent<Button>().onClick.AddListener(() => foto360InsidePoi(button));
                foto360InsidePoi(ListaButtoniInsideGalleria[0]);
                images++;
            }
            else
            {
#if UNITY_EDITOR
                StartCoroutine(GETImage(url, texture =>
                {
                    if (texture != null)
                    {
                        img360.texture = texture;
                        //float altezza = texture.height;
                        //float larghezza = texture.width;
                        //float aspectRatio = altezza / larghezza;
                        spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                        button.transform.GetChild(0).GetComponent<Image>().sprite = spriteImgPrincipale;
                        button.transform.GetChild(0).GetComponent<AspectRatioFitter>().aspectRatio = 2.7f;
                        //Debug.Log(texture.width);
                        ListaButtoniInsideGalleria.Add(button);
                        button.GetComponent<Button>().onClick.AddListener(() => foto360InsidePoi(button));
                        foto360InsidePoi(ListaButtoniInsideGalleria[0]);
                        images++;
                    }
                }));
#else
                WebProxy.GETImage (url, texture =>
                {
                    if (texture != null)
                    {
                        img360.texture = texture;
                        //float altezza = texture.height;
                        //float larghezza = texture.width;
                        //float aspectRatio = altezza / larghezza;
                        spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                        button.transform.GetChild(0).GetComponent<Image>().sprite = spriteImgPrincipale;
                        button.transform.GetChild(0).GetComponent<AspectRatioFitter>().aspectRatio = 2.7f;
                        //Debug.Log(texture.width);
                        ListaButtoniInsideGalleria.Add(button);
                        button.GetComponent<Button>().onClick.AddListener(() => foto360InsidePoi(button));
                        foto360InsidePoi(ListaButtoniInsideGalleria[0]);
                        images++;
                    }
                });
#endif
            }
        }
        //yield return new WaitUntil(() => images == poiInfo.immagini360.Count);
        while (images != poiInfo.immagini360.Count)
        {
            await System.Threading.Tasks.Task.Yield();
        }
        Debug.Log("End 360s");
        LoaderPanel.SetActive(false);
    }

    public void DistruggiDatiCtnPoi(Info poiInfo)
    {
        Debug.Log("DEstroy Poi Contents");
        try
        {
            Equirectangular.SetTexture("_MainTex", null);
            foreach (var b in ListaButtoniInsideGalleria)
            {
                Destroy(b);
            }
            ListaButtoniInsideGalleria.Clear();
            foreach (var i in poiInfo.immagini360)
            {
                Destroy(i.texture);
            }
            foreach (var i in ListaImmaginiGalleria)
            {
                Destroy(i);
            }
            ListaImmaginiGalleria.Clear();
            foreach (var i in poiInfo.immagini_gallery)
            {
                Destroy(i.texture);
            }
            //
            Destroy(poiInfo.immagine_di_copertinaTexture);
            //
            if (audioClip != null)
            {
                Debug.Log("distruggo i contenuti audio");
                currentListening = 0;
                listeningPoint = false;
                Destroy(audioClip);
            }

            //
            //Destroy(immagine_principale.GetComponent<Image>().sprite.texture);
            //Destroy(immagine_principale.GetComponent<Image>().sprite);
            //Destroy(immagine_principaleInfoT.GetComponent<Image>().sprite.texture);
            //Destroy(immagine_principaleInfoT.GetComponent<Image>().sprite);



        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private void foto360InsidePoi(GameObject button)
    {
        Texture texture = button.transform.GetChild(0).GetComponent<Image>().mainTexture;
        Equirectangular.SetTexture("_MainTex", texture);
    }

    public static IEnumerator GETImage(string url, Action<Texture2D> callback)
    {

        Debug.Log(url);
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            ///////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 
            request.certificateHandler = new AcceptAnyCertificate();
            ///////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 

            //Chiamata per l'autorizzazione dell'header
            //AuthorizationAPI.AddAuthRequestHeader(request);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error while reciving: " + request.error);
            }
            else
            {
                try
                {
                    var texture = DownloadHandlerTexture.GetContent(request);
                    texture.name = System.IO.Path.GetFileNameWithoutExtension(url);
                    callback.Invoke(texture);
                }
                catch (Exception ex) { Debug.LogError("Error while assigning the image: " + ex.Message); }
            }
        }
    }

    IEnumerator GetAudioFromFile(string audioFilePath, Action<AudioClip> callback)
    {
        using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(audioFilePath, AudioType.MPEG))
        {
            ///////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 
            audioRequest.certificateHandler = new AcceptAnyCertificate();
            ///////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 
            ///
            yield return audioRequest.SendWebRequest();

            if (audioRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Errore durante il caricamento dell'audio: " + audioRequest.error);
                yield break;
            }

            else
            {
                if (audioClip != null)
                {
                    Destroy(audioClip);
                }
                audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
                if (audioClip != null)
                    audioClip.LoadAudioData();
                callback.Invoke(audioClip);
            }



        }
    }

    public void SendRecipitInfoAssegnaPunti(int content_id, string action)
    {
        AddPoint AddPoint = new AddPoint();
        AddPoint.action = action;
        AddPoint.content_type = "poi";
        AddPoint.content_id = content_id; // <---- CONTENT ID, ATTUALMENTE NON REPERIBILE, QUESTO ID PROVA 01        AddPoint.data_scontrino = recipitDate.text;
                                          //AddPoint.importo = recipitValue.text; AddPoint.numero_scontrino = recipitNumber.text;

        StartCoroutine(ADDPoints(AddPoint, GetRecepitResponse));


        //placeholderPanel.SetActive(true);
        //closeButton.SetActive(true); sendButton.SetActive(false);
    }

    public void ClosePoiInfo()
    {
        dentroUnPOI = false;
    }

    void GetRecepitResponse(ResponseAddPoint response)
    {
        if (response.result == true)
        {
            //actionResponseText.text = response.message + ": " + response.points;
            Debug.Log(response.message + ": " + response.points);
            StartCoroutine(getUserInfo.GETUserData(null));
            popUpPunti.GetComponent<Animator>().Play("Punti");
            popUpPunti.transform.GetChild(1).GetComponent<TMP_Text>().text = response.points;
        }
        else if (response.result == false)
        {
            //actionResponseText.text = response.message + ".";
            Debug.Log(response.message + ". ");


        }
    }


    ///Questa funzione è necessaria per poter fare una richiesta di assegnazione del punteggio, dopo aver visitato un POI 

    public static IEnumerator ADDPoints(AddPoint Addpoint, Action<ResponseAddPoint> callBack)
    {
        string completeUserURL = AuthorizationAPI.baseURL + "/jsonapi" + pointsURL;
        Debug.Log(completeUserURL);
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("user_id", GETUserInfo.DownloadedUserInfo.codice_utente.ToString());
        headers.Add("action", Addpoint.action.ToString());
        headers.Add("content_type", Addpoint.content_type.ToString());
        headers.Add("content_id", Addpoint.content_id.ToString());


        using (UnityWebRequest request = UnityWebRequest.Post(completeUserURL, headers))
        {
            /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 
            request.certificateHandler = new AcceptAnyCertificate();
            /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 

            // Chiamata per l'autorizzazione dell'header 
            AuthorizationAPI.AddAuthRequestHeader(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Utente, " + "Error while reciving: " + request.error);
            }
            else
            {
                //////////////////////////////////////// 
                //Debug.Log(request.downloadHandler.text); 
                //////////////////////////////////////// 

                //Debug.Log(request.downloadHandler.text); 

                string json = request.downloadHandler.text;


                Debug.Log(json);

                RequestResponse = JsonUtility.FromJson<ResponseAddPoint>(json);

                callBack.Invoke(RequestResponse);

                Debug.Log("Esito operazione: " + RequestResponse.message.ToString());
            }
        }
    }

    public void OpenCurrentUrl()
    {
        if (lastPoi != null && !string.IsNullOrEmpty(lastPoi.url))
        {
            Debug.LogWarning(lastPoi.url);
            Application.OpenURL(lastPoi.url);
        }
    }


}
