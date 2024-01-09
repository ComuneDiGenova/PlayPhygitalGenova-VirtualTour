using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class FPSCounter : MonoBehaviour
{
    TMPro.TextMeshProUGUI text;
    float time, frames, fps;

    private void Awake()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
    }
    private void Update()
    {
        time += Time.deltaTime;
        frames++;
        if (time > 1)
        {
            fps = frames / time;
            text.text = fps.ToString("0");
            time = 0;
            frames = 0;
        }
    }
}
