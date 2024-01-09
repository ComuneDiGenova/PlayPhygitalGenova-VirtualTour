using UnityEngine;

public class SphereInstantiator : MonoBehaviour
{
    public GameObject spherePrefab; // Prefab della sfera da istanziare
    public TextAsset dataFile; // File di dati .txt
    public float scale = 0.001f; // Scale factor for coordinate normalization

    private void Start()
    {
        // Leggi il contenuto del file di dati
        string[] lines = dataFile.text.Split('\n');

        foreach (string line in lines)
        {
            // Rimuovi eventuali caratteri di spaziatura
            string trimmedLine = line.Trim();

            if (!string.IsNullOrEmpty(trimmedLine))
            {
                // Divide la riga in coordinate separate
                string[] coordinates = trimmedLine.Split(',');

                if (coordinates.Length == 3)
                {
                    // Converti le coordinate in valori float
                    float x = float.Parse(coordinates[0]) * scale;
                    float y = float.Parse(coordinates[1]) * scale;
                    float z = float.Parse(coordinates[2]) * scale;

                    // Normalizza le coordinate
                    Vector3 normalizedCoordinates = new Vector3(x, y, z);

                    // Istanzia la sfera con il collider nelle coordinate normalizzate
                    InstantiateSphere(normalizedCoordinates);
                }
            }
        }
    }

    private void InstantiateSphere(Vector3 position)
    {
        // Istanzia la sfera prefab alla posizione specificata
        GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);

        // Aggiungi il componente SphereCollider alla sfera
        SphereCollider collider = sphere.AddComponent<SphereCollider>();

        // Personalizza il collider come desiderato
        collider.radius = 0.5f;
        collider.isTrigger = true;

        // Aggiungi altri componenti o personalizzazioni se necessario
    }
}
