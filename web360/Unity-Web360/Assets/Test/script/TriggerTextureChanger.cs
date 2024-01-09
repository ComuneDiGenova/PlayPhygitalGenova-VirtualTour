using System.Collections;
using UnityEngine;

public class TriggerTextureChanger : MonoBehaviour
{
    public GameObject playerObject; // Riferimento all'oggetto Player che contiene la Main Camera e la sfera
    public TriggerTextureData[] triggerData; // Array dei dati per ogni zona di trigger

    private Renderer sphereRenderer; // Renderer della sfera

    public float transitionDuration = 1f; // Durata della transizione in secondi
    public float delayBeforeTransition = 2f; // Ritardo prima dell'inizio della transizione
    public float blurIntensity = 1f; // Intensità della sfocatura durante la transizione

    private Coroutine transitionCoroutine; // Coroutine per gestire la transizione

    private void Start()
    {
        // Ottieni il Renderer della sfera dalla gerarchia dell'oggetto Player
        sphereRenderer = playerObject.GetComponentInChildren<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Controlla se il trigger ha il tag "TRIG1"
        if (other.CompareTag("TRIG1"))
        {
            // Trova i dati corrispondenti per la zona di trigger corrente
            TriggerTextureData data = GetTriggerData(other.gameObject);

            if (data != null)
            {
                // Interrompi la transizione corrente, se presente
                if (transitionCoroutine != null)
                {
                    StopCoroutine(transitionCoroutine);
                }

                // Avvia la nuova transizione
                transitionCoroutine = StartCoroutine(TransitionMaterial(data.triggerMaterial));
            }
        }
    }

    private TriggerTextureData GetTriggerData(GameObject triggerObject)
    {
        // Cerca i dati corrispondenti per il triggerObject
        foreach (TriggerTextureData data in triggerData)
        {
            if (data.triggerObject == triggerObject)
            {
                return data;
            }
        }

        return null;
    }

    private IEnumerator TransitionMaterial(Material targetMaterial)
    {
        // Memorizza il materiale attuale
        Material currentMaterial = sphereRenderer.material;

        // Imposta l'opacità del materiale attuale a 0
        Color currentColor = currentMaterial.GetColor("_Tint");
        currentColor.a = 0f;
        sphereRenderer.material.SetColor("_Tint", currentColor);

        // Applica la sfocatura al materiale attuale
        float currentBlur = 0f;
        currentMaterial.SetFloat("_Blur", currentBlur);

        // Calcola la velocità di transizione
        float transitionSpeed = 1f / transitionDuration;

        // Ritardo prima dell'inizio della transizione
        yield return new WaitForSeconds(delayBeforeTransition);

        // Esegui la transizione gradualmente
        while (currentColor.a < 1f)
        {
            // Aggiorna l'opacità in base al tempo trascorso
            currentColor.a += Time.deltaTime * transitionSpeed;

            // Applica il colore al materiale della sfera
            sphereRenderer.material.SetColor("_Tint", currentColor);

            // Aggiorna la sfocatura in base all'opacità
            currentBlur = currentColor.a * blurIntensity;
            currentMaterial.SetFloat("_Blur", currentBlur);

            yield return null;
        }

        // Assicurati di impostare il materiale di destinazione alla fine della transizione
        sphereRenderer.material = targetMaterial;

        // Resetta la sfocatura
        currentMaterial.SetFloat("_Blur", 0f);

        // Resetta la coroutine
        transitionCoroutine = null;
    }
}
