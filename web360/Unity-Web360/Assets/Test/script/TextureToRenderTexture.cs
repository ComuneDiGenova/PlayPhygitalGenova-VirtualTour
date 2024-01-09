using UnityEngine;

public class TextureToRenderTexture : MonoBehaviour
{
    public Texture2D sourceTexture;
    public RenderTexture targetRenderTexture;

    void Start()
    {
        // Crea una nuova render texture con le stesse dimensioni della texture di origine
        targetRenderTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, 0);
        Graphics.Blit(sourceTexture, targetRenderTexture);
    }
}
