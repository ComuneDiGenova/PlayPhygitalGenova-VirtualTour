using UnityEngine;
using UnityEngine.UI;

public class CTRL_AUDIO : MonoBehaviour
{
    public Button button;
    public Sprite pausa;
    public Sprite play;
    // Start is called before the first frame update
    AudioSource audiosource;
    AudioManager audioManager;
    public static bool isPaused = false;



    private void Start()
    {
        audioManager = gameObject.GetComponent<AudioManager>();
        audiosource = gameObject.GetComponent<AudioSource>();
        button.onClick.AddListener(() => PlayStopAudio());
        isPaused = true;
    }

    public void Play(AudioClip clip)
    {
        audiosource.clip = clip;
        //await System.Threading.Tasks.Task.Yield();
        //Debug.Log("Lunghezza dell'audioclip = " + clip.length);
        button.GetComponent<Image>().sprite = play;
        audiosource.Play();
    }

    private void Update()
    {
        if (!audioManager.audioSource.isPlaying)
        {
            button.GetComponent<Image>().sprite = pausa;
        }
        else
        {
            button.GetComponent<Image>().sprite = play;
        }


    }


    public void PlayStopAudio()
    {
        if (audiosource.clip != null)
        {
            if (audiosource.isPlaying)
            {
                audiosource.Pause();
                isPaused = true;
                audioManager.slideAreaInput.value = audioManager.slideAreaRead.value;
            }
            else
            {
                audiosource.Play();
                isPaused = false;
                audioManager.slideAreaInput.value = audioManager.slideAreaRead.value;
            }
        }
    }
}
