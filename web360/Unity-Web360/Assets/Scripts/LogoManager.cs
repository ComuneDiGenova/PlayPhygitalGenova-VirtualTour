using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LogoManager : MonoBehaviour
{
    const string imageURL = "/sites/default/files/Icona_G.png";
    [SerializeField] Image logoImage;

    private void Start()
    {
        //image url
        GETImage(imageURL, (texture) =>
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2, 100);
            if (texture != null)
                logoImage.sprite = sprite;
        });
    }

    public static async void GETImage(string endpoint, Action<Texture2D> callBack)
    {
        string url = AuthorizationAPI.baseURL + endpoint;
        Debug.Log("GET Image: " + url);
        //LoadingPanel.OpenPanel();

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            AuthorizationAPI.AddAuthRequestHeader(uwr);
            AuthorizationAPI.AddAnyCertificateHandler(uwr);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(endpoint + " | " + uwr.error);
                Debug.LogError(uwr.downloadHandler.text);
                //RemoteErrorLog.LogError("Cannot Download Logo Image",endpoint,uwr.error,uwr.downloadHandler.text);
            }
            else
            {
                //ok
                var txt = DownloadHandlerTexture.GetContent(uwr);
                txt.name = System.IO.Path.GetFileName(url);
                callBack?.Invoke(txt);
            }
        }
        //LoadingPanel.ClosePanel();
    }
}
