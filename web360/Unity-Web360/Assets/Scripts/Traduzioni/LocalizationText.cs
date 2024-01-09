using TMPro;
using UnityEngine;

public class LocalizationText : MonoBehaviour
{
    public string key;

    void Start()
    {

        ChangeLanguage(DropDownValue.dropdownValue);

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }

    void ChangeLanguage(int index)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            var trad = CSVParser.GetTextFromID(key, index);
            if (!string.IsNullOrEmpty(trad))
                gameObject.GetComponent<TMP_Text>().text = trad;
        }
    }

    void OnEnable()
    {
        LanguageDropdown.ChangeLanguage += ChangeLanguage;
        ChangeLanguage(DropDownValue.dropdownValue);
    }

    void OnDisable()
    {
        LanguageDropdown.ChangeLanguage -= ChangeLanguage;
    }
}
