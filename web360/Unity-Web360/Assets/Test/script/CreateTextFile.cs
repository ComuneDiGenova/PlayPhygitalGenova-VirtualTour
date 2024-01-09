using System.IO;
using UnityEngine;

public class CreateTextFile : MonoBehaviour
{
    void Start()
    {
        // Percorso del file .txt (puoi modificarlo secondo le tue esigenze)
        string filePath = Application.dataPath + "/testfile.txt";

        // Dati da scrivere nel file .txt
        string textData = "Hello, World!";

        // Scrivi i dati nel file .txt
        File.WriteAllText(filePath, textData);

        Debug.Log("File .txt creato con successo.");
    }
}
