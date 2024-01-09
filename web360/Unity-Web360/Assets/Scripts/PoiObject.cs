using System;
using UnityEngine;

public class PoiObject : MonoBehaviour
{
    Renderer renderer;
    Action buttonEvent;

    private void Awake()
    {
        renderer = GetComponentInChildren<Renderer>();
    }

    public void Init(Texture2D texture, System.Action action = null)
    {
        renderer.material.mainTexture = texture;
        buttonEvent = action;
    }

    public void Click()
    {
        buttonEvent?.Invoke();
    }
}
