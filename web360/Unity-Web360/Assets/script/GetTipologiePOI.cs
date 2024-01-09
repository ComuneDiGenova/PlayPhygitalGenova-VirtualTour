using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetTipologiePOI : MonoBehaviour
{
    public static event VoidEventhandler OnTipologiePoiTrovate;
    string completeURL;
    string poiTipologiaEndPoint = "/get_tipologie_poi";
    [SerializeField] ListaTipologiePOI listaTipologiePoi;
    [SerializeField] private GameObject ctnParent;
    [SerializeField] private GameObject tipologiaPOIPrefab;
    public static Dictionary<string, Sprite> idToSprite = new Dictionary<string, Sprite>();
    [SerializeField] private Sprite defaultImage;
    //Sprite spriteImgPrincipale;
    public int contatore = 0;
    string id;
    public bool hoScaricato = false;
    // Start is called before the first frame update
    void Start()
    {
        CtrlCoordinate.OnPercorso += StartIstanzioTipologiePOI;
    }

    private void StartIstanzioTipologiePOI()
    {
        Debug.Log("parte la coroutine per GetTipologiePoi");
        StartCoroutine(GETTipologiePOI());

    }

    IEnumerator IstanzioTipologiePOI()
    {
        Debug.Log("inizia la coroutine per istanziare le tipologie");
        //StartCoroutine(GETTipologiePOI());
        yield return new WaitUntil(() => hoScaricato);
        Debug.Log("abbiamo ottenuto una lista di tipologia");
        if (listaTipologiePoi != null && listaTipologiePoi.listaTipologiePoi != null)
        {
            foreach (var tipoPoi in listaTipologiePoi.listaTipologiePoi)
            {
                GameObject newPrefab = Instantiate(tipologiaPOIPrefab);
                newPrefab.transform.SetParent(ctnParent.transform);
                newPrefab.transform.localScale = new Vector3(1, 1, 1);
                newPrefab.GetComponentInChildren<TMP_Text>().text = tipoPoi.nome;
                string url = tipoPoi.icona;
                string id = tipoPoi.id;
                Debug.Log("parte la chiamata al proxy per scaricare le immagini");
//#if UNITY_EDITOR

                if (string.IsNullOrEmpty(url))
                {
                    Debug.Log("la stringa dell'icona � nulla");
                    //url = "https://upload.wikimedia.org/wikipedia/commons/3/33/Vanamo_Logo.png";
                    newPrefab.transform.GetChild(1).GetComponent<Image>().sprite = defaultImage;
                    AddSprite(id, defaultImage);
                    Debug.Log(tipoPoi.id);
                    contatore++;

                }

                //yield return new WaitForSeconds(0.1f);
                else
                {
                    WebProxy.GETImage(url, texture =>
                    {
                        if (texture != null)
                        {
                            var spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                            newPrefab.transform.GetChild(1).GetComponent<Image>().sprite = spriteImgPrincipale;
                            Debug.Log("Ho scaricato l'icona tipologia: " + texture.name);
                            //aggiungo un elemento alla mia dictionary delle categorie
                            AddSprite(id, spriteImgPrincipale);
                            Debug.Log(tipoPoi.id);
                            contatore++;
                        }
                    });
                }
                
//#else
//                Debug.Log("sto per scaricare l'icona ma prima ho lo yield return");
//                yield return true;
//                Debug.Log("sto per scaricare l'icona"+ url);
//                WebProxy.GETImage(url, texture =>
//                {
//                    if (texture != null)
//                    {
//                        var spriteImgPrincipale = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
//                        newPrefab.transform.GetChild(1).GetComponent<Image>().sprite = spriteImgPrincipale;
//                        Debug.Log("Ho scaricato l'icona tipologia: "+ texture.name);
//                        //aggiungo un elemento alla mia dictionary delle categorie
//                        AddSprite(id, spriteImgPrincipale);
//                        Debug.Log(tipoPoi.id);
//                        contatore++;
//                    }
//                });
//#endif

            }

            yield return new WaitUntil(() => contatore == listaTipologiePoi.listaTipologiePoi.Count);

            Debug.Log("Numero di elementi nel dictionary: " + idToSprite.Count);
            Debug.Log("listaTipologiePoi.listaTipologiePoi.Count: " + listaTipologiePoi.listaTipologiePoi.Count);
        }

        yield return null;


        Debug.Log("Il contatore delle tipologie dei poi ha finito");
        OnTipologiePoiTrovate?.Invoke();

    }

    private IEnumerator GETTipologiePOI()
    {
        completeURL = AuthorizationAPI.baseURL + poiTipologiaEndPoint;
        //#if UNITY_EDITOR
        //using (UnityWebRequest request = UnityWebRequest.Get(completeURL))
        //{
        //    /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 
        //    request.certificateHandler = new AcceptAnyCertificate();
        //    /////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO ///////////////////////////////////// 

        //    // Chiamata per l'autorizzazione dell'header 
        //    AuthorizationAPI.AddAuthRequestHeader(request);

        //    yield return request.SendWebRequest();
        //    Debug.Log("Richiesta Tipologie poi : "+ request);
        //    if (request.result == UnityWebRequest.Result.ConnectionError)
        //    {
        //        Debug.Log("Error while reciving: " + request.error);
        //    }
        //    else
        //    {
        //        //////////////////////////////////////// 
        //        //Debug.Log(request.downloadHandler.text); 
        //        //////////////////////////////////////// 

        //        string json = "{\"listaTipologiePoi\" : " + request.downloadHandler.text + "}";
        //        Debug.Log(json);
        //        listaTipologiePoi = JsonUtility.FromJson<ListaTipologiePOI>(json);
        //        Debug.Log(listaTipologiePoi);
        //        //infopois = DownloadedInformationPois;
        //    }
        //}
        Debug.Log("proxy per le tipologie dei poi");
        WebProxy.GetApi(poiTipologiaEndPoint, (response) =>
        {
            string json = "{\"listaTipologiePoi\" : " + response + "}";
            Debug.Log("Il json delle categorie dei POI: " + json);
            listaTipologiePoi = (JsonUtility.FromJson<ListaTipologiePOI>(json));
            Debug.Log("Ho scaricato le tipologie dei poi");
            hoScaricato = true;
        }, null);
        yield return new WaitUntil(() => hoScaricato);
        StartCoroutine(IstanzioTipologiePOI());
        //#else
        //        yield return true;
        //        WebProxy.GetApi(poiTipologiaEndPoint, (response) => {
        //            string json = "{\"listaTipologiePoi\" : " + response + "}";
        //            Debug.Log(json);
        //            listaTipologiePoi = (JsonUtility.FromJson<ListaTipologiePOI>(json));
        //            hoScaricato = true;
        //        },null);
        //#endif
    }

    // Aggiungi una nuova entry (id, sprite) nel dictionary
    public static void AddSprite(string id, Sprite sprite)
    {
        idToSprite[id] = sprite;
    }

    // Restituisci la sprite associata a un determinato ID
    public static Sprite GetSprite(string id)
    {
        if (idToSprite.ContainsKey(id))
        {
            return idToSprite[id];
        }
        else
        {
            Debug.LogWarning("ID non trovato nel dictionary: " + id);
            return null;
        }
    }

}
