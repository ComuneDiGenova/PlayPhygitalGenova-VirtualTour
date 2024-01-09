using Cinemachine;
using UnityEngine;

public class ToggleCinemachineInput : MonoBehaviour
{
    CinemachineInputProvider inputProvider;

    public bool startActive = false;
    [Header("Debug")]
    public bool isDragging = false;
    public bool isInside = false;

    private void Start()
    {
        // Ottieni il componente CinemachineInputProvider
        inputProvider = GetComponent<CinemachineInputProvider>();
        inputProvider.enabled = false;
    }

    public void SetCinemachineInput(bool active)
    {
        //Debug.Log(active);
        isInside = active;
        if (!active)
            isDragging = false;
    }

    private void OnEnable()
    {
        isInside = startActive;
    }


    void Update()
    {
        if (!isInside) return;

        // if we press the left mouse button or touch and we haven't started dragging
        if ((Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)) && !isDragging)
        {
            // set the flag to true
            isDragging = true;
            inputProvider.enabled = true;
        }
        else if ((Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Ended)) && isDragging)
        {
            // set the flag to false
            isDragging = false;
            inputProvider.enabled = false;
        }

    }


}