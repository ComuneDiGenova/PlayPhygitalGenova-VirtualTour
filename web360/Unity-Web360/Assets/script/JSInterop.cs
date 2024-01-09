using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

public class JSInterop : MonoBehaviour
{
    // Start is called before the first frame update
    [DllImport("__Internal")]
    public static extern void CloseWindow();

    [DllImport("__Internal")]
    public static extern string GetURLFromPage();

    [DllImport("__Internal")]
    public static extern string SyncFiles();
    [DllImport("__Internal")]
    public static extern string ReadFiles();

    //
    public delegate void VoidEvent();
    public static event VoidEvent OnSyncEnded;

    public delegate void ItinerarioEvent(ItinerarioJS itinerario);
    public static event ItinerarioEvent OnItinerario;

    public static ItinerarioJS itinerarioJS = new ItinerarioJS();

    public static string pageBaseUrl;

    //public static string GetBaseUrl(string url)
    //{
    //    if (!string.IsNullOrEmpty(url))
    //    {
    //        Uri uri = new Uri(url);
    //        string baseUrl = uri.GetLeftPart(UriPartial.Authority) + "/";
    //        Debug.Log("Page Base Url = " + baseUrl);
    //        return baseUrl;
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}

    public static string GetBaseUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                // Definisci il pattern Regex per trovare la prima barra dopo l'inizio dell'URL
                //string pattern = @"^(https?://[^/]+/)";
                var pattern = @"(https?://[[a-zA-Z0-9.~]+)"; //-> include solo i caratteri abilitati per i domini
                Regex regex = new Regex(pattern);

                // Cerca la corrispondenza nel URL
                Match match = regex.Match(url);

                if (match.Success)
                {
                    // Ottieni il valore corrispondente alla prima barra dopo l'inizio dell'URL
                    string baseUrl = match.Groups[1].Value;
                    Debug.Log("Page Base Url = " + baseUrl);
                    return baseUrl;
                }else{
                    return null; //-> se no match torno null
                }

                // -> NON SERVE! il forward slash no nveve mai esserci nel base url! il regex già lo esclude
                // Se non c'è una barra dopo l'inizio dell'URL, restituisci l'intero URL
                //Debug.Log("Page Base Url = " + url);
                //return url;
            }
            catch (Exception e)
            {
                Debug.LogError("Error while processing URL: " + e.Message);
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    private void Awake()
    {
        gameObject.name = "[JSInterop]";
        Debug.LogWarning("VERSIONE: " + Application.version);
    }

    private void Start()
    {
#if UNITY_EDITOR
        itinerarioJS = new ItinerarioJS()
        {
            nome = "demo",
            lingua = "it",
            id = "16892599758",
            rgb = "#000000",

            // via balbi 

            //lista_poi = new List<PoiJS>{
            //    new PoiJS(){    //viene trovato da listapois
            //        id_poi = "8.9255896,44.4153081" 
            //        //id_poi = "8.930714761257642,44.407769126284165"
            //    },
            //     new PoiJS(){    //viene trovato da listapois
            //       id_poi = "8.9278802,44.4141030"
            //       // id_poi = "8.928238273401437,44.407289759779125"
            //    },
            //     new PoiJS(){    //viene trovato da listapois
            //       id_poi = "8.927605,44.412058"
            //       // id_poi = "8.928238273401437,44.407289759779125"
            //    }

            //}

            //lista_poi = new List<PoiJS>{
            //    new PoiJS(){    //viene trovato da listapois
            //        id_poi = "1555"
            //    },
            //     new PoiJS(){    //viene trovato da listapois
            //        id_poi = "1543"
            //    },
            //     new PoiJS(){    //viene trovato da listapois
            //        id_poi = "1622"
            //    },
            //    new PoiJS(){    //viene scartato xè non trovato
            //        id_poi = "null"
            //    },
            //    new PoiJS(){    //viene creato poi custom da coordinate
            //        //id_poi = "1558"
            //        //id_poi = "1622"
            //        id_poi = "8.934023811525375,44.40773809151876"
            //    }
            //}


            //lista_poi = new List<PoiJS>{
            //    new PoiJS(){    
            //        id_poi = "9974"
            //    },
            //     new PoiJS(){    
            //        id_poi = "dfdg"
            //    },
            //     new PoiJS(){    
            //        id_poi = "1574"
            //    }
            //}

            lista_poi = new List<PoiJS>{
                new PoiJS(){
                    id_poi = "8.9337315, 44.4073802"
                },
                new PoiJS(){
                    id_poi = "8.9330019 ,44.4067785"
                },
                new PoiJS(){
                    id_poi = "8.9309125, 44.4076623"
                },
                new PoiJS(){
                    id_poi = "8.9318070, 44.4086470"
                },
                new PoiJS(){
                    id_poi = "8.9310358, 44.4096636"
                },
                new PoiJS(){
                    id_poi = "8.930526, 44.410744"
                },
                new PoiJS(){
                    id_poi = "8.9298986, 44.4104383"
                },
                new PoiJS(){
                    id_poi = "8.92981160, 44.4094515"
                }
            }

        };
        OnItinerario.Invoke(itinerarioJS);
        //let itinere = `{"nome":"demo","id":16892599758,"lingua":"IT","lista_poi":[{"id_poi":"1555"},{"id_poi":"1543"},{"id_poi":"1622"},{"id_poi":"null"},{"id_poi":"8.934023811525375,44.40773809151876"}]}`;
#endif
    }

    public static void CloseApplication()
    {
        Debug.Log("Closing Application");
        CloseWindow();
    }

    public void Itinerario(string json)
    {
        Debug.Log("itinerario: " + json);
        itinerarioJS = JsonUtility.FromJson<ItinerarioJS>(json);
        var url = GetURLFromPage();
        Debug.Log("la pagina che ospita l'app ha il seguente link : " + url);
        var lang = url.Split("=");

        if (lang.Length == 2)
        {
            switch (lang[1])
            {
                case "it":
                    itinerarioJS.lingua = "Italiano"; break;
                case "en":
                    itinerarioJS.lingua = "Inglese"; break;
                case "fr":
                    itinerarioJS.lingua = "Francese"; break;
                case "de":
                    itinerarioJS.lingua = "Tedesco"; break;
                case "es":
                    itinerarioJS.lingua = "Spagnolo"; break;
                case "ru":
                    itinerarioJS.lingua = "Russo"; break;
                default:
                    itinerarioJS.lingua = "Italiano"; break;
            }
        }else{
            itinerarioJS.lingua = "Italiano";
        }
        Debug.Log("La lingua scelta è : " + itinerarioJS.lingua);
        OnItinerario.Invoke(itinerarioJS);
    }

    public void EndSync()
    {
        OnSyncEnded?.Invoke();
    }
}