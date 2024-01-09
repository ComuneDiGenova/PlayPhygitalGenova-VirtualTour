using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class testAudioall : MonoBehaviour
{
    public AudioClip embeddedClip;
    public string streamingClipName;
    public string remoteClipUrl;

    AudioClip streaming, remote;
    AudioSource as_embedded, as_streaming, as_remote;

    private void Start()
    {
        as_embedded = gameObject.AddComponent<AudioSource>();
        as_streaming = gameObject.AddComponent<AudioSource>();
        as_remote = gameObject.AddComponent<AudioSource>();
    }

    public void StopAll()
    {
        as_embedded.Stop();
        as_streaming.Stop();
        as_remote.Stop();
    }

    public void Embedded()
    {
        as_embedded.clip = embeddedClip;
        as_embedded.Play();
    }

    public async void StreamingAsset()
    {
        if (streaming != null)
        {
            as_streaming.clip = streaming;
            as_streaming.Play();
            return;
        }
        string url = Application.streamingAssetsPath + "/" + streamingClipName;
#if UNITY_EDITOR
        url = "file:/" + url;
#endif
        Debug.Log(url);
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.ACC))
        {
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
                Debug.Log(uwr.downloadProgress);
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                streaming = DownloadHandlerAudioClip.GetContent(uwr);
                if (streaming != null)
                {
                    streaming.LoadAudioData();
                    as_streaming.clip = streaming;
                    as_streaming.Play();
                }
            }
        }
    }

    public async void Remote()
    {
        if (remote != null)
        {
            as_remote.clip = remote;
            as_remote.Play();
            return;
        }
        string url = remoteClipUrl;
        Debug.Log(url);
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
                Debug.Log(uwr.downloadProgress);
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                remote = DownloadHandlerAudioClip.GetContent(uwr);
                if (remote != null)
                {
                    remote.LoadAudioData();
                    as_remote.clip = remote;
                    as_remote.Play();
                }
            }
        }
    }
}
