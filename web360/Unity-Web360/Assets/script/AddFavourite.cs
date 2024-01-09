using System.Linq;
using UnityEngine;
public class AddFavourite : MonoBehaviour
{
    //[SerializeField] GameObject favouriteLogInTab;
    public static event VoidEventhandler OnControlloPreferiti;
    public static event VoidEventhandler OnAssegnoPreferiti;


    //[SerializeField] Sprite notFavouriteSprite;
    //[SerializeField] Sprite favouriteSprite;
    public static string favouriteClickedMarkerLabel;
    public static bool favouritePoi = false;
    GETPointOfInterest getPointOfInterest;

    private void Start()
    {
        getPointOfInterest = GameObject.Find("CTRL_Coordinate").GetComponent<GETPointOfInterest>();
    }

    public void checkFavourite()
    {
        if (GETUserInfo.DownloadedUserInfo == null || GETUserInfo.DownloadedUserInfo.codice_utente == "0" || GETUserInfo.FavouriteResponse == null) return;
        var favouritePoiTrovato = GETUserInfo.FavouriteResponse.favourites.Where((x) => (x.id == favouriteClickedMarkerLabel)).FirstOrDefault();
        if (favouritePoiTrovato != null && favouritePoiTrovato.id != null)
        {
            Debug.Log(favouriteClickedMarkerLabel);
            Debug.Log("Preferito trovato: " + favouritePoiTrovato.id);
        }
        else
        {
            Debug.Log("Nesusn preferito");
        }


        //qua controlliamo se siamo loggati


        if (favouritePoiTrovato == null)
        {
            favouritePoi = false;
            OnControlloPreferiti.Invoke();
        }
        else
        {
            favouritePoi = true;
            OnControlloPreferiti.Invoke();
        }
    }

    public void AddToFavourite()
    {
        if (GETUserInfo.DownloadedUserInfo == null || GETUserInfo.DownloadedUserInfo.codice_utente == "0" || GETUserInfo.FavouriteResponse == null) return;
        Debug.Log(favouriteClickedMarkerLabel);
        var favouritePoiTrovato = GETUserInfo.FavouriteResponse.favourites.Where((x) => (x.id.ToString() == favouriteClickedMarkerLabel)).FirstOrDefault();
        Debug.Log("Preferito trovato: " + favouritePoiTrovato);

        //qua controlliamo se siamo loggati

        if (GETUserInfo.DownloadedUserInfo.codice_utente != "0")
        {
            if (favouritePoiTrovato == null)
            {
                StartCoroutine(POSTFavourites.POSTAddFavourite(favouriteClickedMarkerLabel, GetAddFavouriteResponse));
            }
            else
            {
                StartCoroutine(POSTFavourites.POSTRemoveFavourite(favouriteClickedMarkerLabel, GetRemoveFavouriteResponse));
            }

        }
        if (/*!GameConfig.isLogged*/ GETUserInfo.DownloadedUserInfo.codice_utente == "0")
        {
            //favouriteLogInTab.SetActive(true);
            Debug.Log("l'utente non � loggato");
        }
    }
    public void CloseFavLogInTab()
    {
        //favouriteLogInTab.SetActive(false);
    }
    void GetAddFavouriteResponse(AddFavouriteResponse response)
    {
        if (response.result == true)
        {
            Debug.Log("Ho istanziato il cuoricino");
            StartCoroutine(GETUserInfo.GETFavourites(null));
            favouritePoi = true;
            OnAssegnoPreferiti.Invoke();
            string action = "preferiti";
            getPointOfInterest.SendRecipitInfoAssegnaPunti(int.Parse(favouriteClickedMarkerLabel), action);

            //favouriteImage.sprite = favouriteSprite;
        }
        else if (response.result == false)
        {
            Debug.Log(response.message + ".");
            StartCoroutine(GETUserInfo.GETFavourites(null));
            favouritePoi = false;
            OnAssegnoPreferiti.Invoke();
        }
    }
    void GetRemoveFavouriteResponse(RemoveFavouriteResponse response)
    {
        if (response.result == true)
        {
            Debug.Log("Ho tolto il cuoricino");
            StartCoroutine(GETUserInfo.GETFavourites(null));
            favouritePoi = false;
            OnAssegnoPreferiti.Invoke();

            //favouriteImage.sprite = notFavouriteSprite;
        }
        else if (response.result == false)
        {
            favouritePoi = true;
            Debug.Log(response.message + ".");
            StartCoroutine(GETUserInfo.GETFavourites(null));
            OnAssegnoPreferiti.Invoke();
        }
    }
}