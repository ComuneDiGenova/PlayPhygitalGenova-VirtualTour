using UnityEngine;

public class SphereTextureChanger : MonoBehaviour
{
    public Material defaultMaterial;  // Il materiale predefinito
    public Material collisionMaterial;  // Il materiale per la collisione

    private Renderer sphereRenderer;  // Renderer della sfera

    private void Start()
    {
        // Ottieni il Renderer della sfera
        sphereRenderer = GetComponentInChildren<Renderer>();

        // Applica il materiale predefinito alla sfera
        sphereRenderer.material = defaultMaterial;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Controlla se la collisione è avvenuta con una delle zone di collisione desiderate
        if (collision.gameObject.CompareTag("CollisionZone"))
        {
            // Applica il materiale per la collisione alla sfera
            sphereRenderer.material = collisionMaterial;
        }
    }
}
