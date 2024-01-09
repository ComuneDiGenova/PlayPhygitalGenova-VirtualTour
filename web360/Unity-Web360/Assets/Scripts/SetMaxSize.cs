using UnityEngine;

public class SetMaxSize : MonoBehaviour
{
    [SerializeField] [Range(0.01f, 1f)] float maxSizeOnScreen = 0.3f;
    public float refSize = 1;
    float minDistance = 0;
    public MeshRenderer renderer;

    private void Awake()
    {
        renderer = GetComponentInChildren<MeshRenderer>();
    }

    private void Update()
    {
        transform.LookAt(new Vector3(0f, 0f, Camera.main.transform.position.z), Vector3.up);
        //
        var min = Camera.main.WorldToViewportPoint(renderer.bounds.min);
        var max = Camera.main.WorldToViewportPoint(renderer.bounds.max);
        Debug.Log($"{renderer.bounds} -> {min} | {max}");
        var size = (max - min).magnitude;

        if (size > maxSizeOnScreen)
        {
            if (minDistance == 0)
                minDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
            Debug.Log($"{name} : {size} / {maxSizeOnScreen} - {minDistance}");
            SetSize();
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }
    public void SetSize()
    {
        //if(gameObject.activeSelf)
        //Debug.Log("resize");
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        float scale = distance / minDistance;
        transform.localScale = Vector3.one * scale * refSize;
    }
}
