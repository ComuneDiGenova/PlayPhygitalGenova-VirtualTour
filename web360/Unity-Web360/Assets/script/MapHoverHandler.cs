using UnityEngine;
using UnityEngine.EventSystems;

public class MapHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject mapMask; // Riferimento all'oggetto che contiene la maschera dell'immagine
    private bool isPointerOverMap;

    private void Update()
    {
        if (isPointerOverMap && Input.GetMouseButton(0))
        {
            // Gestisci l'evento di hover del mouse solo se il puntatore del mouse si trova sopra l'area visibile della mappa
            // E l'utente sta tenendo premuto il pulsante sinistro del mouse
            HandleMapHover();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOverMap = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverMap = false;
    }

    private void HandleMapHover()
    {
        // Gestisci l'evento di hover del mouse sulla mappa qui
        Debug.Log("Mouse hover over the map");
    }
}
