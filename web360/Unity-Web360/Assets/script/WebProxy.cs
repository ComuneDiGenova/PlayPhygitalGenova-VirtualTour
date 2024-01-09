using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebProxy
{
    private const string proxyPageApi = "/fetch_api_call.php";  //gli endpoint iniziano SEMPRE con il forwardSLash!
    private const string proxyPageGet = "/proxy_get.php"; //gli endpoint iniziano SEMPRE con il forwardSLash!
    public static string baseUrl = null;


    //private void Awake()
    //{
    //    CheckBaseUrl();
    //}
    public static void CheckBaseUrl(){
#if UNITY_EDITOR

        var page="" ;
        baseUrl = JSInterop.GetBaseUrl(page);
#else
        if(string.IsNullOrEmpty(baseUrl)){
            string page = JSInterop.GetURLFromPage();
            baseUrl = JSInterop.GetBaseUrl(page);
        }
        if(string.IsNullOrEmpty(baseUrl))
            throw new Exception("Missing page url Fron JS");
#endif
      
        Debug.Log("Proxy Base Url = " + baseUrl);
    }
    
    public static async void GetLocalText(string url, Action<string> callBack, Action errorCallback = null)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            AuthorizationAPI.AddAnyCertificateHandler(uwr);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {uwr.responseCode} - {uwr.error}");
                errorCallback?.Invoke();
            }
            else
            {
                Debug.Log(uwr.downloadHandler.text);
                callBack.Invoke(uwr.downloadHandler.text);
            }
        }
    }

    public static async void Get(string url, Action<string> callBack, Action errorCallback = null)
    {
        CheckBaseUrl();
        string proxyUrl = baseUrl + proxyPageGet + "?url=" + url;
        //Dictionary<string,string> headers = new Dictionary<string, string>();
        //headers.Add("method", "GET");
        Debug.Log($"{url} - {proxyUrl}");
        using (UnityWebRequest uwr = UnityWebRequest.Get(proxyUrl))
        {
            AuthorizationAPI.AddAnyCertificateHandler(uwr);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {url} -  {uwr.responseCode} - {uwr.error}");
                Debug.LogError(uwr.downloadHandler.text);
                errorCallback?.Invoke();
            }
            else
            {
                Debug.Log(uwr.downloadHandler.text);
                callBack.Invoke(uwr.downloadHandler.text);
            }
        }
    }
    public static async void GetDirect(string url, Action<string> callBack, Action errorCallback = null)
    {
        Debug.Log($"{url}");
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            AuthorizationAPI.AddAnyCertificateHandler(uwr);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {url} - {uwr.responseCode} - {uwr.error}");
                Debug.LogError(uwr.downloadHandler.text);
                errorCallback?.Invoke();
            }
            else
            {
                Debug.Log(uwr.downloadHandler.text);
                callBack.Invoke(uwr.downloadHandler.text);
            }
        }
    }
    public static async void GetApi(string endpoint, Action<string> callBack, Action errorCallback = null)
    {
        CheckBaseUrl();
        string proxyUrl = baseUrl + proxyPageApi + "?endpoint=" + endpoint;
        // Dictionary<string,string> headers = new Dictionary<string, string>();
        //headers.Add("method", "GET");
        Debug.Log($"api dal proxy :{endpoint} - {proxyUrl}");
        using (UnityWebRequest uwr = UnityWebRequest.Get(proxyUrl))
        {
            AuthorizationAPI.AddAnyCertificateHandler(uwr);
            AuthorizationAPI.AddAuthRequestHeader(uwr);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
                //Debug.Log("aspetto uwr");
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {proxyUrl} - {uwr.responseCode} - {uwr.error}");
                Debug.LogError(uwr.downloadHandler.text);
                errorCallback?.Invoke();
            }
            else
            {
                Debug.Log(uwr.downloadHandler.text);
                callBack.Invoke(uwr.downloadHandler.text);
                //Debug.Log("Ho scaricato il testo");
            }
        }
    }

    public static async void PostApi(string endpoint, Dictionary<string, string> request_headers, Action<string> callBack, Action errorCallback = null)
    {
        CheckBaseUrl();
        string proxyUrl = baseUrl + proxyPageApi;
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("method", "POST");
        headers.Add("endpoint", endpoint);
        foreach (var kvp in request_headers)
        {
            headers.Add(kvp.Key, kvp.Value);
        }
        Debug.Log($"{proxyUrl} - {endpoint}");
        using (UnityWebRequest uwr = UnityWebRequest.Post(proxyUrl, headers))
        {
            AuthorizationAPI.AddAnyCertificateHandler(uwr);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {proxyUrl} - {uwr.responseCode} - {uwr.error}");
                Debug.LogError(uwr.downloadHandler.text);
                errorCallback?.Invoke();
            }
            else
            {
                Debug.Log(uwr.downloadHandler.text);
                callBack.Invoke(uwr.downloadHandler.text);
            }
        }
    }

    public static async void GETImage(string url, Action<Texture2D> callBack, Action errorCallback = null)
    {
        CheckBaseUrl();
        string proxyUrl = baseUrl + proxyPageGet + "?url=" + url;
        using (UnityWebRequest uwr = UnityWebRequest.Get(proxyUrl))
        {
            Debug.Log($"{proxyUrl} - {url}");
            AuthorizationAPI.AddAnyCertificateHandler(uwr);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {proxyUrl} -> {uwr.responseCode} - {uwr.error}");
                Debug.LogError(uwr.downloadHandler.text);
                errorCallback?.Invoke();
                callBack.Invoke(null);
            }
            else
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false, false);
                texture.filterMode = FilterMode.Bilinear;
                texture.name = System.IO.Path.GetFileNameWithoutExtension(url);
                try
                {
                    texture.LoadImage(uwr.downloadHandler.data);
                    callBack.Invoke(texture);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    callBack.Invoke(null);
                }
            }
        }
    }


    public static async void GetAudio(string url, Action<AudioClip> callback, Action errorCallback = null)
    {
        CheckBaseUrl();
        string proxyUrl = baseUrl + proxyPageGet + "?url=" + url;
        Dictionary<string, string> headers = new Dictionary<string, string>();
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(proxyUrl, AudioType.MPEG))
        {
            AuthorizationAPI.AddAnyCertificateHandler(uwr);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Errore durante il caricamento dell'audio: " + uwr.error);
                Debug.LogError(uwr.downloadHandler.text);
                errorCallback?.Invoke();
            }
            else
            {
                try
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);
                    audioClip.LoadAudioData();
                    callback.Invoke(audioClip);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
        }
    }




}
