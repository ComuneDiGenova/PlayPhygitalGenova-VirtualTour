
//using ARLocation.Utils;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GETShops : MonoBehaviour
{
    //[SerializeField] InstantiateShopOnMap instantiateShop;
    [SerializeField] GameObject poiPrefab;
    [SerializeField] Transform shopParent;
    string apiURL = "/get_shop_list";
    static string shopDeteilsEndpoint = "/post_shop_details";
    public CTRL_Player ctrl_Player;
    public static ShopInformationList DownloadedInformationShop = new ShopInformationList();
    [SerializeField] ShopInformationList infoshops; //DA RIMUOVERE !!!!

    public ToggleCinemachineInput toggleinputCamera;
    public CinemachineInputProvider inputProvider;



    // contentui del POI turistico
    [Space(10)]
    [Header("contentui del POI Shop")]
    [SerializeField] GameObject ctn_poiShop;
    [SerializeField] GameObject BoxBianco;
    [SerializeField] GameObject immagine_principale;
    [SerializeField] TMP_Text TXT_titolo;
    [SerializeField] TMP_Text TXT_categoria;
    [SerializeField] TMP_Text TXT_descrizione;
    [SerializeField] TMP_Text TXT_indirizzo;
    [SerializeField] TMP_Text TXT_orario;
    [SerializeField] TMP_Text TXT_telefono;
    [SerializeField] TMP_Text TXT_sito;
    [SerializeField] TMP_Text TXT_mail;
    [SerializeField] Button Esplora;
    [SerializeField] GameObject PrefabImageGallery;
    [SerializeField] GameObject ParentGallery;
    [SerializeField] GameObject ContentDescrizione;

    [Space(10)]
    [Header("contentui del POI Shop Interno")]

    [SerializeField] GameObject immagine_FullScreen;
    [SerializeField] GameObject ctn_poiShopInterno;
    [SerializeField] TMP_Text TXT_titoloInt;
    [SerializeField] TMP_Text TXT_categoriaInt;
    [SerializeField] TMP_Text TXT_descrizioneInt;
    [SerializeField] TMP_Text TXT_indirizzoInt;
    [SerializeField] TMP_Text TXT_orarioInt;
    [SerializeField] TMP_Text TXT_telefonoInt;
    [SerializeField] TMP_Text TXT_sitoInt;
    [SerializeField] TMP_Text TXT_mailInt;
    [Space(10)]

    private List<GameObject> ListaImmaginiGalleria = new List<GameObject>();
    public List<GameObject> ListaShop = new List<GameObject>();
    [SerializeField] AddFavourite addFavourite;
    public ShopInformaitions lastPoi;
    public List<Sprite> listaIcone = new List<Sprite>();

    Sprite spriteImgPrincipale;
    private int iconIndex;
    private Vector2 pozioneDefaultContentScroll;
    public GameObject ctrl_mappa;
    public GameObject loader;
    public GETPointOfInterest getPointOfInterest;
    void Start()
    {
        CtrlCoordinate.OnPercorso += inizioRicercaShop;
        pozioneDefaultContentScroll = ContentDescrizione.transform.localPosition;
        getPointOfInterest = gameObject.GetComponent<GETPointOfInterest>();
    }

    private void inizioRicercaShop()
    {
        StartCoroutine(GETShopList());
    }

    IEnumerator GETShopList()
    {
        string url = AuthorizationAPI.baseURL + "/jsonapi" + apiURL;
#if UNITY_EDITOR
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO /////////////////////////////////////
            request.certificateHandler = new AcceptAnyCertificate();
            /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO /////////////////////////////////////

            Debug.Log(url);

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

                string json = "{\"shopInfos\" : " + request.downloadHandler.text + "}";
                Debug.Log(json);
                DownloadedInformationShop = (JsonUtility.FromJson<ShopInformationList>(json));
                infoshops = DownloadedInformationShop;
            }
            InstantiateShop();
        }
#else
    yield return true;
    WebProxy.GetApi(apiURL, (response) => {
        string json = "{\"shopInfos\" : " + response + "}";
        Debug.Log(json);
        DownloadedInformationShop = (JsonUtility.FromJson<ShopInformationList>(json));
        infoshops = DownloadedInformationShop;
        InstantiateShop();
    },null);

#endif
    }

    void InstantiateShop()
    {
        //yield return StartCoroutine(GETInfoPois());
        Debug.LogWarning("Istanzio Shops");
        if (DownloadedInformationShop != null)
        {
            foreach (var p in DownloadedInformationShop.shopInfos)
            {
                if (p.ToCoordinate() == null)
                    continue;
                //Traduco le coordinate e istanzio i poi nel mondo 3D/////////////////////
                var posizione = GeoCoordinate.Utils.GeoToUtm(p.ToCoordinate());
                var poi = Instantiate(poiPrefab, shopParent);
                poi.name = p.id;
                poi.transform.SetParent(shopParent);
                var newPosition = posizione - CtrlCoordinate.start;
                poi.transform.position = newPosition.ToVector3();
                GetPOIType(p.tipologia);
                poi.GetComponent<Image>().sprite = listaIcone[iconIndex];
                var pp = poi.AddComponent<TEST_SHOP_POI>();
                pp.Inizialize(p);
                poi.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ApriShop(poi, p)));
                ListaShop.Add(poi);

                ////setto l'immagine corretta in base alla categoria//
                //GetPOIType(p.id_tipologia);
                //poi.GetComponent<Image>().sprite = listaIcone[iconIndex];
                //if (iconIndex != 2)
                //{
                //    poi.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ApriInfoPoiTuristico(poi)));
                //}
                //else poi.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ApriContenutoShop(poi)));
            }
        }
    }

    public void GetPOIType(string type)
    {
        switch (type)
        {
            case "Botteghe Storiche": // acquedotto storico
                iconIndex = 0;
                break;
            default:
                iconIndex = 1;
                break;
        }
    }

    public static IEnumerator GETExtendedInfo(ShopShortInfo shortInfo, Action<ShopInformaitions> callBack)
    {
        string url = AuthorizationAPI.baseURL + "/jsonapi" + shopDeteilsEndpoint;

        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("nid", shortInfo.id.ToString());
        //headers.Add("language", ((Languages)DropDownValue.dropdownValue).ToString().ToLower());
        //headers.Add("languages", GameConfig.applicationLanguage.ToString()); // Inseriamo la lingua nell'header.
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
            }
            else
            {
                //Debug.Log(request.downloadHandler.text); 

                string json = "{\"shopInfoList\" : " + request.downloadHandler.text + "}";

                //Debug.Log("dopo: " + json); 
                Debug.Log(json);
                var info = JsonUtility.FromJson<ShopInformationsList>(json).shopInfoList[0];
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
                            callBack.Invoke(info);
                            yield break;
                        }
                        string json2 = "{\"shopInfoList\" : " + response2 + "}";
                        Debug.Log(json2);
                        var infolist2 = JsonUtility.FromJson<ShopInformationsList>(json2);
                        if (infolist2 != null && infolist2.shopInfoList != null && infolist2.shopInfoList.Count == 1)
                        {
                            var info2 = infolist2.shopInfoList[0];
                            callBack.Invoke(info2);
                        }
                        else
                        {
                            callBack.Invoke(info);
                        }
                    }
                }
                //DownloadedShopExtendedInfo.Add(shortInfo, info);
                //callBack.Invoke(info);
                //OnExtendedInfo?.Invoke(shortInfo);
            }
        }
#else
    yield return true;
    WebProxy.PostApi(shopDeteilsEndpoint, headers,(response) => {
        string json = "{\"shopInfoList\" : " + response + "}";
        //Debug.Log("dopo: " + json); 
        Debug.Log(json);
        var info = JsonUtility.FromJson<ShopInformationsList>(json).shopInfoList[0];
        //
        headers.Add("language", ((Languages)DropDownValue.dropdownValue).ToString().ToLower());
        WebProxy.PostApi(shopDeteilsEndpoint,headers,(response2) => {
            if(response2 == "[]"){
                Debug.LogWarning($"NO Language {((Languages)DropDownValue.dropdownValue).ToString().ToLower()} for {shortInfo.id.ToString()}");
                callBack.Invoke(info);
                return;
            }
            string json2 = "{\"infos\" : " + response2+ "}";
            Debug.Log("dopo: " + json2);
            var infolist2 = JsonUtility.FromJson<ShopInformationsList>(json2);
            if(infolist2 != null && infolist2.shopInfoList != null && infolist2.shopInfoList.Count == 1){
                var info2 = infolist2.shopInfoList[0];
                callBack.Invoke(info2);
            }else{
                callBack.Invoke(info);
            }
        },null);
        //DownloadedShopExtendedInfo.Add(shortInfo, info);
        //callBack.Invoke(info);
    },null);

#endif
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //---------------------------APRI PAGINA NEGOZIO----------------------------------------------------/////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    IEnumerator ApriShop(GameObject poiObject, ShopShortInfo poiInfo)
    {
        loader.SetActive(true);
        GETPointOfInterest.dentroUnPOI = true;
        toggleinputCamera.enabled = false;
        inputProvider.enabled = false;

        // mettiamo in pausa il tour virtuale
        if (ctrl_Player.startPlayer)
        {
            ctrl_Player.PlayPauseShop();
        }
        //chiudiamo la mappa 
        ctrl_mappa.GetComponent<Animator>().SetTrigger("close");

        //distruggiamo le immagini di galleria se presenti
        //DistruggiDatiCtnPoi();
        ContentDescrizione.transform.localPosition = pozioneDefaultContentScroll;

        //facciamo la chiamata per prendere il dato relativo al POI
        TEST_SHOP_POI poiComponent = poiObject.GetComponent<TEST_SHOP_POI>();
        //var shortInfo = poiComponent.shortInfoPoi;
        poiComponent.SetInfo();
        Debug.Log("aspettiamo il caricamento delle info negozio");
        yield return new WaitUntil(() => poiComponent.infoShopPoi != null);

        if (poiComponent.infoShopPoi != lastPoi)
            DistruggiDatiCtnPoi(lastPoi);
        lastPoi = poiComponent.infoShopPoi;


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////7
        /////////////////////////// MODIFICHIAMO UN POI MANUALMENTE, SARA DA ELIMINARE QUESTA PARTE QUA /////////////////
        ///



        // Ora puoi accedere al componente InfoPOI per ottenere le informazioni

        Debug.Log("assegnamo i valori a tutti i testi del negozio");
        // Assegna i testi

        TXT_titolo.text = poiComponent.infoShopPoi.nome;
        TXT_titoloInt.text = poiComponent.infoShopPoi.nome;
        string newText = TextDecoder.DecodeText(poiComponent.infoShopPoi.descrizione);
        TXT_descrizioneInt.text = newText;
        TXT_descrizione.text = newText;
        TXT_categoria.text = poiComponent.infoShopPoi.tipologia;
        TXT_categoriaInt.text = poiComponent.infoShopPoi.tipologia;
        TXT_indirizzoInt.text = poiComponent.infoShopPoi.indirizzo;
        TXT_indirizzo.text = poiComponent.infoShopPoi.indirizzo;
        //TXT_orario.text = poiComponent.infoShopPoi.orari;
        //TXT_orarioInt.text = poiComponent.infoShopPoi.orari;
        TXT_telefono.text = poiComponent.infoShopPoi.telefono;
        TXT_telefonoInt.text = poiComponent.infoShopPoi.telefono;
        TXT_sito.text = $"<link={poiComponent.infoShopPoi.url}> vai al sito </link>";
        TXT_sitoInt.text = $"<link={poiComponent.infoShopPoi.url}> vai al sito </link>";
        TXT_mail.text = poiComponent.infoShopPoi.email;
        TXT_mailInt.text = poiComponent.infoShopPoi.email;
        Debug.Log("abbiamo assegnato i testi");

        Debug.Log($"numero di immagini 360 = {poiComponent.infoShopPoi.immagini360.Count}");
        if (poiComponent.infoShopPoi.immagini360.Count > 0)
        {
            Esplora.interactable = true;
            Esplora.onClick.RemoveAllListeners();
            Esplora.onClick.AddListener(() => getPointOfInterest.EsploraContenutoTuristico(poiObject, poiComponent.infoShopPoi));
            Esplora.gameObject.SetActive(true);
        }
        else
        {
            Esplora.interactable = false;
            Esplora.gameObject.SetActive(false);
        }


        ctn_poiShop.SetActive(true);
        loader.SetActive(false);
        // controlliamo i preferiti
        AddFavourite.favouriteClickedMarkerLabel = poiInfo.id.ToString();
        //Debug.Log(AddFavourite.favouriteClickedMarkerLabel); 

        if ((GETUserInfo.DownloadedUserInfo != null && GETUserInfo.DownloadedUserInfo.codice_utente != "0"))
        {
            addFavourite.checkFavourite();
        }

        //facciamo la chimata per cercare l'immagine di copertina
        if (!string.IsNullOrEmpty(poiComponent.infoShopPoi.immagine_di_copertina))
        {
            string url = poiComponent.infoShopPoi.immagine_di_copertina;
            if (poiComponent.infoShopPoi.immagine_di_copertinaTexture != null)
            {
                float altezza = poiComponent.infoShopPoi.immagine_di_copertinaTexture.height;
                float larghezza = poiComponent.infoShopPoi.immagine_di_copertinaTexture.width;
                float aspectRatio = larghezza / altezza;
                Debug.Log("aspect Ratio dell'immagine già in memoria" + aspectRatio);
                spriteImgPrincipale = Sprite.Create(poiComponent.infoShopPoi.immagine_di_copertinaTexture, new Rect(0, 0, larghezza, altezza), Vector2.one * 0.5f);
                immagine_principale.GetComponent<Image>().sprite = spriteImgPrincipale;
                immagine_FullScreen.GetComponent<Image>().sprite = spriteImgPrincipale;
                immagine_principale.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                immagine_FullScreen.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
            }
            else
            {
#if UNITY_EDITOR
                StartCoroutine(GETPointOfInterest.GETImage(url, texture =>
                {
                    if (texture != null)
                    {
                        poiComponent.infoShopPoi.immagine_di_copertinaTexture = texture;
                        float altezza = texture.height;
                        float larghezza = texture.width;
                        float aspectRatio = larghezza / altezza;
                        Debug.Log("aspect Ratio dell'immagine scaricata da unity" + aspectRatio);
                        spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                        immagine_principale.GetComponent<Image>().sprite = spriteImgPrincipale;
                        immagine_FullScreen.GetComponent<Image>().sprite = spriteImgPrincipale;
                        immagine_principale.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                        immagine_FullScreen.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                    }
                    else Debug.LogError("nessuna texture trovata");
                }));
#else
            yield return true;
            WebProxy.GETImage(url, texture =>
            {
                if (texture != null)
                {
                    poiComponent.infoShopPoi.immagine_di_copertinaTexture = texture;
                    float altezza = texture.height;
                    float larghezza = texture.width;
                    float aspectRatio =  larghezza/altezza;
                    Debug.Log("aspect Ratio dell'immagine scaricata via proxy" + aspectRatio);
                    spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    immagine_principale.GetComponent<Image>().sprite = spriteImgPrincipale;
                    immagine_FullScreen.GetComponent<Image>().sprite = spriteImgPrincipale;
                    immagine_principale.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                    immagine_FullScreen.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                }
                else Debug.LogError("nessuna texture trovata");
            });
#endif
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)BoxBianco.transform);

        }

        //////prendiamo le immagini della gallery del poi///////////////

        //Debug.Log("nuimero elementi immagini gallery " + poiInfo.immagini_gallery.Count);
        //        if (poiComponent.infoShopPoi.immagini_gallery.Count > 0)
        //        {
        //            //Debug.Log("qua dovrebbe arrivare");
        //            foreach (Immagini_Gallery imageGallery in poiComponent.infoShopPoi.immagini_gallery)
        //            {
        //                string url = imageGallery.immagine_gallery;
        //                if(imageGallery.texture != null){
        //                    float altezza = imageGallery.texture.height;
        //                    float larghezza = imageGallery.texture.width;
        //                    float aspectRatio = altezza / larghezza;
        //                    spriteImgPrincipale = Sprite.Create(imageGallery.texture, new Rect(0, 0, imageGallery.texture.width, imageGallery.texture.height), Vector2.one * 0.5f);
        //                    var imgGallery= Instantiate(PrefabImageGallery, new Vector3(0, 0), Quaternion.identity);
        //                    imgGallery.transform.SetParent(ParentGallery.transform, false);
        //                    imgGallery.GetComponent<Image>().sprite = spriteImgPrincipale;
        //                    //imgGallery.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
        //                    //Debug.Log(texture.width);
        //                    ListaImmaginiGalleria.Add(imgGallery);
        //                }else{
        //#if UNITY_EDITOR
        //                StartCoroutine(GETPointOfInterest.GETImage(url, texture =>
        //                {
        //                    //Debug.Log("vediamo se cicla");
        //                    if (texture != null)
        //                    {
        //                        imageGallery.texture = texture;
        //                        float altezza = texture.height;
        //                        float larghezza = texture.width;
        //                        float aspectRatio = altezza / larghezza;
        //                        spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        //                        var imgGallery = Instantiate(PrefabImageGallery, new Vector3(0, 0), Quaternion.identity);
        //                        imgGallery.transform.SetParent(ParentGallery.transform, false);
        //                        imgGallery.GetComponent<Image>().sprite = spriteImgPrincipale;
        //                        ListaImmaginiGalleria.Add(imgGallery);
        //                    }
        //                    else Debug.LogError("Nessuna texture trovata");
        //                }));

        //#else
        //            yield return true;
        //            WebProxy.GETImage(url, texture =>
        //            {
        //                if (texture != null)
        //                    {
        //                        imageGallery.texture = texture;
        //                        float altezza = texture.height;
        //                        float larghezza = texture.width;
        //                        float aspectRatio = altezza / larghezza;
        //                        spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        //                        var imgGallery = Instantiate(PrefabImageGallery, new Vector3(0, 0), Quaternion.identity);
        //                        imgGallery.transform.SetParent(ParentGallery.transform, false);
        //                        imgGallery.GetComponent<Image>().sprite = spriteImgPrincipale;
        //                        ListaImmaginiGalleria.Add(imgGallery);
        //                    }
        //                    else Debug.LogError("Nessuna texture trovata");
        //                });
        //#endif
        //                }
        //            }

        //        }

        //assegnamo un audio se esiste
        //if (!string.IsNullOrEmpty(poiComponent.infoPOI.audio))
        //{
        //    string audioUrl = poiComponent.infoPOI.audio;
        //    string avatar = poiComponent.infoPOI.avatar;
        //    Debug.Log(audioUrl);
        //    if (avatar == "0")
        //    {
        //        AvatarPaganini.SetActive(true);
        //        AvatarDuchessa.SetActive(false);
        //    }
        //    else { AvatarDuchessa.SetActive(true); AvatarPaganini.SetActive(false); }

        //    yield return StartCoroutine(GetAudioFromFile(audioUrl, audioclip =>
        //    {

        //        Debug.Log($"La callback funziona");
        //        if (audioclip != null)
        //        {
        //            ctrl_audio.SetActive(true);
        //            ctrl_audio.GetComponent<AudioSource>().clip = audioClip;
        //            ctrl_audio.GetComponent<AudioSource>().Play();
        //            slider.value = 0;

        //        }
        //        else Debug.LogError("Nessuna texture trovata");
        //    }));
        //}

        //Esplora.onClick.RemoveAllListeners();
        //Esplora.gameObject.SetActive(true);
        //Esplora.onClick.AddListener(() => EsploraShop(poiObject, lastPoi));
        /*
        if(!string.IsNullOrEmpty(poiComponent.infoShopPoi.immagini360)){
            Esplora.gameObject.SetActive(true);
            Esplora.onClick.AddListener(() => EsploraShop(poiObject, lastPoi));
        }else{
            Esplora.gameObject.SetActive(false);
        }*/
    }

    private void EsploraShop(GameObject poiObject, ShopInformaitions poiInfo)
    {
        ctn_poiShopInterno.SetActive(true);
        ctn_poiShop.SetActive(false);
        AddFavourite.favouriteClickedMarkerLabel = poiInfo.id.ToString();
        //Debug.Log(AddFavourite.favouriteClickedMarkerLabel); 

        if ((GETUserInfo.DownloadedUserInfo != null && GETUserInfo.DownloadedUserInfo.codice_utente != "0"))
        {
            addFavourite.checkFavourite();
        }


        //}


    }

    public void DistruggiDatiCtnPoi(ShopInformaitions poiInfo)
    {
        Debug.Log("DEstroy Poi Contents");
        try
        {

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
        }
        catch (Exception e)
        {
            Debug.Log(e);
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
