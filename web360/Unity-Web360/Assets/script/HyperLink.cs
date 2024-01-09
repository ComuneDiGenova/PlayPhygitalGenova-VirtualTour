using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HyperLink : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text yourTextMeshPro; // Riferimento al componente TextMeshPro che contiene il testo con i link.

    private void Start()
    {
        yourTextMeshPro = gameObject.GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(yourTextMeshPro, Input.mousePosition, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = yourTextMeshPro.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();

            // Apri l'URL nel browser predefinito del sistema.
            Application.OpenURL(url);
        }
    }
}