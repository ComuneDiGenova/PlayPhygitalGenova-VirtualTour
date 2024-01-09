using UnityEngine;

public class testdistance : MonoBehaviour
{
    public float refScale = 1;
    public float refdist = 1;

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
        float scale = dist / refdist;
        transform.localScale = Vector3.one * scale * refScale;
    }
}
