using UnityEngine;
using UnityEngine.UI;

public class BottonePreferiti : MonoBehaviour
{
    //[SerializeField] GameObject favouriteLogInTab;
    Button button;
    [SerializeField] GameObject cuoricino;
    [SerializeField] GameObject AggiungiAipreferiti;
    [SerializeField] GameObject AggiuntoAiPreferiti;
    [SerializeField] AddFavourite addFavourite;
    [SerializeField] GameObject UtenteNonLoggato;
    // Start is called before the first frame update

    private void Start() {
        button = gameObject.GetComponent<Button>();
        if (GETUserInfo.DownloadedUserInfo != null && GETUserInfo.DownloadedUserInfo.codice_utente != "0")
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => addFavourite.AddToFavourite());
            button.onClick.AddListener(() => AssegnoPreferito());
            //Debug.Log("ho aggiunto AddToFAvourite");
        }
        else
        {
            button.onClick.AddListener(() => MostraMessaggioDiLog());
            //Debug.Log("ho aggiunto mostraMessaggioLog");
        }
    }
    private void OnEnable()
    {
        
        button.enabled = true;

    }
    private void Awake()
    {
        AddFavourite.OnControlloPreferiti += ControlloPreferito;
        AddFavourite.OnAssegnoPreferiti += ControlloCuoricinoServer;
    }




    // Update is called once per frame
    public void ControlloPreferito()
    {
        if (!AddFavourite.favouritePoi)
        {
            cuoricino.SetActive(false);
            AggiungiAipreferiti.SetActive(true);
            AggiuntoAiPreferiti.SetActive(false);
        }
        else
        {
            cuoricino.SetActive(true);
            AggiungiAipreferiti.SetActive(false);
            AggiuntoAiPreferiti.SetActive(true);
        }
    }


    //questo mi serve per dare un feedback immediato all'utente che ha premuito il bottone.
    public void AssegnoPreferito()
    {
        if (!AddFavourite.favouritePoi)
        {
            cuoricino.SetActive(true);
            cuoricino.GetComponent<Animation>().Play("Preferiti");
            AggiungiAipreferiti.SetActive(false);
            AggiuntoAiPreferiti.SetActive(true);
            button.enabled = false;
        }
        else
        {
            cuoricino.SetActive(true);
            cuoricino.GetComponent<Animation>().Play("NoPreferiti");
            AggiungiAipreferiti.SetActive(true);
            AggiuntoAiPreferiti.SetActive(false);
            button.enabled = false;
        }
    }

    //questo mi serbve per controllare e assegnare il cuoricino in base alla risposta reale del server.
    public void ControlloCuoricinoServer()
    {
        if (AddFavourite.favouritePoi)
        {
            cuoricino.SetActive(true);
            AggiungiAipreferiti.SetActive(false);
            AggiuntoAiPreferiti.SetActive(true);
            button.enabled = true;
        }
        else
        {
            cuoricino.SetActive(false);
            AggiungiAipreferiti.SetActive(true);
            AggiuntoAiPreferiti.SetActive(false);
            button.enabled = true;

        }
    }



    public void MostraMessaggioDiLog()
    {
        UtenteNonLoggato.GetComponent<Animator>().Play("AnimazionePreferiti");
        Debug.Log("Per aggiungere ai preferiti devi eseguire l'accesso");
    }

}
