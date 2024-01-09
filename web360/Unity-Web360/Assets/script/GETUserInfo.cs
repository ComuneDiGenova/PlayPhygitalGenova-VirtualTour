using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GETUserInfo : MonoBehaviour
{
    [SerializeField] UserDataHandler dataHandlerScript;
    string userURL = "/user-get-info";
    static string favouriteURL = "/user/get_preferiti_utente";
    string completeUserURL;
    private bool InizioControlloPreferiti = false;

    public static InfoUser DownloadedUserInfo;
    public static FavouriteList FavouriteResponse;

    // /////////////////////////// // 
    // ////////// FAKE /////////// // 
    // /////////////////////////// // 

    void Awake()
    {
        //StartCoroutine(GETUserData(completeUserURL));
        JSInterop.OnItinerario += (itinerario) =>
        {
            Debug.Log("REperimento dati utente");
            StartCoroutine(GETUserData(() =>
            {
                StartCoroutine(GETFavourites(() =>
                {
                    Debug.Log("ho preso i preferiti utente ");
                    InizioControlloPreferiti = true;
                }));
            }));
        };
    }
    //private void Update()
    //{
    //    if (InizioControlloPreferiti)
    //    {
    //        StartCoroutine(GETFavourites(30));
    //    }

    //}




    public IEnumerator GETUserData(Action callBack)
    {
        completeUserURL = userURL + "/" + JSInterop.itinerarioJS.id;
        Debug.Log(completeUserURL);
#if UNITY_EDITOR
        using (UnityWebRequest request = UnityWebRequest.Get(AuthorizationAPI.baseURL + "/jsonapi" + completeUserURL))
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

                Debug.Log(request.downloadHandler.text);

                string json = request.downloadHandler.text;

                Debug.Log(json);
                json = json.Replace("[", "").Replace("]", "");
                //Debug.Log(json);

                DownloadedUserInfo = JsonUtility.FromJson<InfoUser>(json);

                Debug.Log("Informazioni Utente: " + DownloadedUserInfo.ToString());

                dataHandlerScript.SetData(DownloadedUserInfo.codice_utente, DownloadedUserInfo.nome, DownloadedUserInfo.cognome, DownloadedUserInfo.email, DownloadedUserInfo.genovini);
                dataHandlerScript.SetData(DownloadedUserInfo.nome, DownloadedUserInfo.cognome, DownloadedUserInfo.email);
                //DropDownValue.dropdownValue = InfoUser.GetLanguageIndex(DownloadedUserInfo.lingua); // assegnamo alla variabile globale della lingua.
                //LanguageDropdown.ChangeLanguage(InfoUser.GetLanguageIndex(DownloadedUserInfo.lingua));

                callBack?.Invoke();
            }
        }
#else
    WebProxy.GetApi(completeUserURL,(response) => {
        if(response == "[]"){
            Debug.LogError("Nessun utente trovato");
        }else{
            string json = response.Replace("[", "").Replace("]", "");
            Debug.Log(json);
            DownloadedUserInfo = JsonUtility.FromJson<InfoUser>(json);

            Debug.Log("Informazioni Utente: " + DownloadedUserInfo.ToString());

            dataHandlerScript.SetData(DownloadedUserInfo.codice_utente, DownloadedUserInfo.nome, DownloadedUserInfo.cognome, DownloadedUserInfo.email, DownloadedUserInfo.genovini);
            dataHandlerScript.SetData(DownloadedUserInfo.nome, DownloadedUserInfo.cognome, DownloadedUserInfo.email);
        }
        callBack?.Invoke();
    }, () =>{
        callBack?.Invoke();
    });
    yield return true;
#endif
    }

    public static IEnumerator GETFavourites(Action callBack)
    {
        Debug.Log("Get Favourites");
        if (GETUserInfo.DownloadedUserInfo == null || GETUserInfo.DownloadedUserInfo.codice_utente == "0") yield break;
        /*
        if (GETUserInfo.DownloadedUserInfo != null && DownloadedUserInfo.codice_utente == "0") 
        {
            Debug.LogWarning("il codice utente � uguale a zero quindi l'utente nn � loggato");
            yield break;
        }
        */
        //yield return new WaitForSeconds(timer);
        string completeUserURL = AuthorizationAPI.baseURL + "/jsonapi" + favouriteURL;
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("uid", DownloadedUserInfo.codice_utente.ToString());
        Debug.Log("Ottenendo preferiti");

#if UNITY_EDITOR
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
                //Debug.Log(request.downloadHandler.text);
                string json = "{\"favourites\" : " + request.downloadHandler.text + "}";
                //Debug.Log(json);
                FavouriteResponse = JsonUtility.FromJson<FavouriteList>(json);
                Debug.Log("Esito ottenimento preferiti: " + json);
                callBack?.Invoke();
            }
        }
#else
        WebProxy.PostApi(favouriteURL, headers,(response) => {
            string json = "{\"favourites\" : " + response + "}";
            // Debug.Log(json);
            FavouriteResponse = JsonUtility.FromJson<FavouriteList>(json);
            Debug.Log("Esito ottenimento preferiti: " + json);
            callBack?.Invoke();
        },null);
        yield return new WaitUntil(() => FavouriteResponse != null);
#endif
    }

}