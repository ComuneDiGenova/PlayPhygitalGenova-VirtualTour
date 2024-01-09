using UnityEngine;

public class SizeDistance : MonoBehaviour
{
    public float refDistance = 40;
    public float refSize = 1;
    public float minScaleFactor = 0.1f;  // Fattore di scala minimo
    public bool update = false;

    void LateUpdate()
    {
        if (update)
            SetSize();
    }

    public void SetSize()
    {
        //if(gameObject.activeSelf)
        //Debug.Log("resize");
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        float scale = distance / refDistance;
        //Debug.Log(distance);
        if (distance >= refDistance)
        {
            scale = 1.0f - (distance - refDistance) / refDistance * (1.0f - minScaleFactor);
            scale = Mathf.Max(minScaleFactor, scale);  // Imposta il minimo a minScaleFactor
            transform.localScale = Vector3.one * scale * refSize;
        }
        else
        {
            scale = scale > 1 ? 1 : scale;
            transform.localScale = Vector3.one * scale * refSize;
        }

    }
}
