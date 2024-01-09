using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RemoteErrorLog
{
    const string LogEndpoint = null;
    static bool useProxy = true;

    public static void LogError(string message){
        if (string.IsNullOrEmpty(LogEndpoint)) return;
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("message", message);
#if UNITY_EDITOR
        PostLog(LogEndpoint,headers,() => {
            Debug.LogError($"Cannot remote log error: {message}");
        });
#else
        WebProxy.PostApi(LogEndpoint, headers, null, () => {
            Debug.LogError($"Cannot remote log error: {message}");
        });
#endif
    }
    public static void LogError(string message, string endpoint_url, string error_code, string error_response){
        if (string.IsNullOrEmpty(LogEndpoint)) return;
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("message", message);
        headers.Add("endpoint", endpoint_url);
        headers.Add("err_code", error_code);
        headers.Add("err_response", error_response);
#if UNITY_EDITOR
        PostLog(LogEndpoint,headers,() => {
            Debug.LogError($"Cannot remote log error: {message}");
        });
#else
        WebProxy.PostApi(LogEndpoint, headers, null, () => {
            Debug.LogError($"Cannot remote log error: {message}");
        });
#endif
    }

    private static async void PostLog(string endpoint, Dictionary<string, string> request_headers, Action errorCallback = null){
        string url = AuthorizationAPI.baseURL + LogEndpoint;
        using (UnityWebRequest uwr = UnityWebRequest.Post(url, request_headers))
        {
            uwr.certificateHandler = new AcceptAnyCertificate();
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"REMOTE LOG Error: {url} - {uwr.responseCode} - {uwr.error}");
                Debug.LogError($"Error message: '{request_headers["message"]}'");
                Debug.LogError(uwr.downloadHandler.text);
                errorCallback?.Invoke();
            }
            else
            {
                Debug.Log(uwr.downloadHandler.text);
                //ok
            }
        }
    }
}
