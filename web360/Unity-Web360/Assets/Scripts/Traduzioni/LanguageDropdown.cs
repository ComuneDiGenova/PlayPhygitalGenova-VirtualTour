using System;
using TMPro;
using UnityEngine;

public class LanguageDropdown : MonoBehaviour
{
    static public Action<int> ChangeLanguage;
    public TMP_Dropdown dropdown;
    public TMP_Text label;

    public void LanguageChanged()
    {
        if (ChangeLanguage != null)
        {
            DropDownValue.dropdownValue = dropdown.value;
            ChangeLanguage(dropdown.value);
        }

        dropdown.captionText.text = CSVParser.GetAvailableLanguages()[dropdown.value];
        label.text = dropdown.captionText.text;
    }

    void Start()
    {
        PopulateDropdown();
    }

    void PopulateDropdown()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(CSVParser.GetAvailableLanguages());
        dropdown.value = DropDownValue.dropdownValue;
        LanguageChanged();
    }
}
