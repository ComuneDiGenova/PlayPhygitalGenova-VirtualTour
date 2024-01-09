using UnityEngine;

public class testaudio : MonoBehaviour
{
    [SerializeField] string url = "https://file-examples.com/storage/fee3d1095964bab199aee29/2017/11/file_example_MP3_700KB.mp3";
    public AudioSource source;



    private void Start()
    {
        WebProxy.GetAudio(url, (clip) =>
        {
            source.clip = clip;
            source.Play();
        }, () =>
        {
            //error
        });
    }
}
