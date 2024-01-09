using UnityEngine;

public class ChangeMaterial : MonoBehaviour
{
    public Material[] material;

    Renderer rend;


    private void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = material[0];

    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Trig1")
        {
            rend.sharedMaterial = material[0];
        }

        else if (col.gameObject.tag == "Trig2")
        {
            rend.sharedMaterial = material[1];
        }


        else if (col.gameObject.tag == "Trig3")
        {
            rend.sharedMaterial = material[2];
        }
    }
}
