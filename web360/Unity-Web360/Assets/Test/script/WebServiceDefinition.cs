using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InfoUser
{
    public string codice_utente;
    public string nome;
    public string cognome;
    public string email;
    public string genovini;
    public string lingua;
    public string Qrcode;
    public string Foto;
    public List<string> Lista_preferiti;

    public override string ToString() => $"InfoUser: {codice_utente}; {cognome}; {nome}; {email}; {genovini}; {lingua}; {Qrcode}; {Foto}; { string.Join(",", Lista_preferiti)}";

    static public int GetLanguageIndex(string lingua)
    {
        int languageIndex = 0;
        switch (lingua)
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
        return languageIndex;
    }

}


/// <summary>
/// POI preferiti
/// </summary>

[System.Serializable]
public class UserFavourite
{
    public string id;
    public string nome;
    public string tipo;
    public string categoria;

    public override string ToString()
    {
        return $"{id}, {nome}, {tipo}, {categoria}";
    }
}

[System.Serializable]
public class FavouriteList
{
    public List<UserFavourite> favourites = new List<UserFavourite>();
}

[System.Serializable]
public class AddFavouriteResponse
{
    public bool result;
    public string message;
}

public class RemoveFavouriteResponse
{
    public bool result;
    public string message;
}

/// <summary>
/// / classe per i dettagli del poi
/// </summary>

[System.Serializable]
public class Info
{
    public string id_poi;
    public string nome;
    public string accessibilit;
    public int agevolazioni;
    public string audio;
    public string avatar;
    public string cellulare;
    public string coordinate;
    public string descrizione_audio;
    public string email;
    public string indirizzo;
    public int orari;
    public string servizi;
    public string tags;
    public string telefono;
    public string descrizione;
    public string tipologia;
    public List<Immagini360> immagini360 = new List<Immagini360>();
    public string gallery;
    public List<Immagini_Gallery> immagini_gallery = new List<Immagini_Gallery>();
    public string immagine_di_copertina;
    public string url;
    public int id;
    [System.NonSerialized] public Texture2D immagine_di_copertinaTexture;
    public override string ToString()
    {
        return $"{id}, {nome}, {coordinate}, {cellulare}, {email}, {url}, 360: {immagini360.Count}, img: {immagini_gallery.Count},{descrizione}";
    }
}

[System.Serializable]
public class Immagini360
{
    public string immagine360;
    public string title;
    [System.NonSerialized] public Texture2D texture;
}

[System.Serializable]
public class Immagini_Gallery
{
    public string immagine_gallery;
    public string title;
    [System.NonSerialized] public Texture2D texture;
}

[System.Serializable]
public class InfoList
{
    public List<Info> infos = new List<Info>();
}

[System.Serializable]
public class PointData
{
    public int Id;
    public double X; //lon
    public double Y; //lat
    //public string Direction_;
    //public string Northing;
    //public string Height_;
    //public string Up_Easting;
    //public string Roll_X_deg; //Z
    //public string Pitch_Y_de; //X
    //public string Yaw_Z_deg_; //Y+90
    public string Omega_deg_;
    public string Phi_deg_;
    public string Kappa_deg_;

    public string Filename;

    private GeoCoordinate.Coordinate coordinate = null;

    public GeoCoordinate.Coordinate ToCoordinate()
    {
        if (coordinate == null)
            coordinate = new GeoCoordinate.Coordinate(Y, X);
        return coordinate;
    }

    public override string ToString()
    {
        return $"PointData: lat {Y}, lon {X}, O {Omega_deg_}, P {Phi_deg_}, K {Kappa_deg_}, {Filename}";
    }
}

[System.Serializable]
public class ListaSferiche
{
    public string versione;
    public string Base_url;
    public List<PointData> sferiche = new List<PointData>();
}

[System.Serializable]
public class VersioneSferiche
{
    public string versione;
}


[System.Serializable]
public class InfoJS // questo � il valore salvato nella pagina javascript che l'app WebGL va a LEGGERE
{
    public ItinerarioJS itinerario;
}

[System.Serializable]
public class ItinerarioJS // Classe necessaria per costruire il percorso nella app360 WebGL e che va salvata nelle API 
{
    public string nome;
    public string id;
    public string lingua;
    public string rgb;
    public List<PoiJS> lista_poi;
}

[System.Serializable]
public class PoiJS
{
    public string id_poi;
}


/// esempio fornito da DAvide ///////////////////////
//dati_app_360 = JSON.stringify("
//{
//  "idutente": "1",
//  "itinerario": {
//        "nome": "",
//    "id": 1,
//    "lista_poi": [{
//            "id_poi": 123
//       },
//       {
//            "id_poi": 124
//       }];
//    }
//}
//")

[System.Serializable]
public class ItinerarioDettaglio // questa � la classe riportata dall dettaglio dell'itinerario 
{
    public string id;
    public string nome;
    public string url;
    public string descrizione;
    public string sferiche; // Questo qua salviamo il nostro itineraruio JS in formato JSON

}

[System.Serializable]
public class ItinerarioShort// questa � la classe dentro listaItinerari dalle API 
{
    public string predefinito;
}

[System.Serializable]
public class ListaItinerari// questa � la classe riportata dalla lista degli itinerari delle API 
{
    public List<ItinerarioShort> itinerari = new List<ItinerarioShort>();
}



[System.Serializable]
public class ShortInfo
{
    public string id;
    public string title;
    public double lat;
    public double lon;
    public string id_tipologia;
    public string tipologia;
    public string tipo;

    private GeoCoordinate.Coordinate coordinate = null;

    public GeoCoordinate.Coordinate ToCoordinate()
    {
        if (coordinate == null && lat != 0 || lon != 0)
        {
            coordinate = new GeoCoordinate.Coordinate(lat, lon);
        }
        return coordinate;
    }

    public override string ToString()
    {
        return $"ShortInfo: {id}, {lon}, {lat}, {id_tipologia}, {tipologia}, {tipo}";
    }
}

[System.Serializable]
public class ListaTipologiePOI
{
    public List<TipologiaPoi> listaTipologiePoi = new List<TipologiaPoi>();
}

[System.Serializable]
public class TipologiaPoi
{
    public string id;
    public string nome;
    public string icona;

}

[System.Serializable]
public class InformationList
{
    public List<ShortInfo> infos = new List<ShortInfo>();
}


// //////////////////////////////////////////////////// // 
// ////////////////// INFO SHOP /////////////////////// // 
// //////////////////////////////////////////////////// // 

[System.Serializable]
public class ShopShortInfo
{
    public string id;
    public string title;
    public string tipologia;
    public double lat;
    public double lon;

    private GeoCoordinate.Coordinate coordinate = null;

    public GeoCoordinate.Coordinate ToCoordinate()
    {
        if (coordinate == null && lat != 0 || lon != 0)
        {
            coordinate = new GeoCoordinate.Coordinate(lat, lon);
        }
        return coordinate;
    }
}

[System.Serializable]
public class ShopInformationList
{
    public List<ShopShortInfo> shopInfos = new List<ShopShortInfo>();
}



[System.Serializable]
public class ShopType
{
    public string id;
    public string nome;
}

[System.Serializable]
public class ShopInformaitions
{
    public string id;
    public string nome;
    public double latitudine;
    public double longitudine;
    public ShopType categoria_del_negozio;
    public string cellulare;
    public string email;
    public string indirizzo;
    public string orari;
    public string tags;
    public string telefono;
    public string descrizione;
    public string tipologia;
    public string immagine_di_copertina;
    public List<Immagini360> immagini360 = new List<Immagini360>();
    public List<Immagini_Gallery> immagini_gallery = new List<Immagini_Gallery>();
    public string audio;
    public string url;
    [System.NonSerialized] public Texture2D immagine_di_copertinaTexture;

    public override string ToString()
    {
        return $"{id}, {nome}, {latitudine}:{longitudine}, {cellulare}, {email}, {url}, img: {immagini_gallery.Count}, {descrizione}";
    }
}

[System.Serializable]
public class ShopInformationsList
{
    public List<ShopInformaitions> shopInfoList = new List<ShopInformaitions>();
}

// //////////////////////////////////////////////////// // 
// //////////////// FINE INFO SHOP ///////////////////// // 
// //////////////////////////////////////////////////// //





/// <summary>
/// ////////Inviare azione punti esplorazione POI
/// </summary>
// POST[System.Serializable]
public class AddPoint
{
    public int user_id;
    public string action;
    public string content_type;
    public int content_id;
    public string data_scontrino; // swagger data scontrino
    public string importo; // swagger segnato come number
    public string numero_scontrino;
}
// GET
[System.Serializable]
public class ResponseAddPoint
{
    public bool result;
    public string message;
    public string points;
    public override string ToString()
    {
        return $"{result}, {message}, {points}";
    }
}

