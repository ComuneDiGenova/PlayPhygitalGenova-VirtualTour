using TMPro;
using UnityEngine;
public class UserDataHandler : MonoBehaviour
{
    //[SerializeField] TMP_Text userID;
    [SerializeField] TMP_Text nameText;//[SerializeField]TMP_Text optionsNameText;
    //[SerializeField] TMP_Text surnameText;[SerializeField] TMP_Text emailText;
    [SerializeField] TMP_Text genoviniText;
    //[SerializeField] TMP_Text optionsNameProText;[SerializeField] TMP_Text surnameProText;
    //[SerializeField] TMP_Text emailProText;

    public void SetData(string id, string name, string surname, string email, string genovini)
    {
        //userID.text = id;
        nameText.text = name;
        //optionsNameText.text = name;
        //surnameText.text = surname; emailText.text = email;
        genoviniText.text = genovini;
    }
    public void SetData(string name, string surname, string email)
    {
        //optionsNameProText.text = name;
        //surnameProText.text = surname; emailProText.text = email;
    }

    public void SetLanguage(string language, out int languageIndex)
    {
        string lingua = language;
        languageIndex = 0;
        switch (language)
        {
            case "Italiano":
                languageIndex = 0; break;
            case "Inglese":
                languageIndex = 1;
                break;
            case "Francese":
                languageIndex = 2; break;
            case "Tedesco":
                languageIndex = 3;
                break;
            case "Spagnolo":
                languageIndex = 4; break;
            case "Russo":
                languageIndex = 5;
                break;
            default:
                languageIndex = 0; break;
        }
    }
}