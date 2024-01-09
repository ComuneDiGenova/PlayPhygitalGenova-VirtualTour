using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public Slider slideAreaInput;
    public Slider slideAreaRead;
    public RectTransform fillArea;


    // Start is called before the first frame update
    void OnStart()
    {
        slideAreaRead.interactable = false;
        //slideAreaInput.onValueChanged.AddListener((float value) => { audioSource.time = value * audioSource.clip.length;});
    }

    void LateUpdate()
    {
        //if(audioSource.clip != null){
        //    Debug.Log("slideAreaRead.value = " + slideAreaRead.value+ " && " + " audioSource.time = "+ audioSource.time);
        //    slideAreaRead.value = audioSource.time / audioSource.clip.length;
        //}else{
        //    //slideAreaRead.value = 0;
        //}
        UpdateSlider();
    }

    public void SetAudio(float value)
    {
        if (audioSource.clip == null) return;
        audioSource.time = value * audioSource.clip.length;
    }

    private void UpdateSlider()
    {
        if (audioSource.clip != null && audioSource.clip.length != 0)
        {
            slideAreaRead.value = audioSource.time / audioSource.clip.length;
            //Debug.Log("Lunghezza dell'audioclip = " + audioSource.clip.length);
            //Debug.Log("slideAreaRead.value = " + slideAreaRead.value + " && " + " audioSource.time = " + audioSource.time);
            LayoutRebuilder.ForceRebuildLayoutImmediate(fillArea);
            // Attendi un frame prima di aggiornare nuovamente
        }
        // Aggiorna il valore della slider


    }

}
