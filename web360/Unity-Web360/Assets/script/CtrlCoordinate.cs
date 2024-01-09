using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class CtrlCoordinate : MonoBehaviour
{
    // delegate per richiamare un evento, una volta terminato di caricare la lista di punti
    static CtrlCoordinate instance;

    public static event VoidEventhandler OnPercorso;

    public float maxWaypointDistance = 5;
    //public static List<GeoCoordinate.Coordinate> coordinateList = new List<GeoCoordinate.Coordinate>();
    public List<GameObject> PuntiList = new List<GameObject>();
    //public List<Texture2D> textures360 = new List<Texture2D>();
    public static Dictionary<GeoCoordinate.Coordinate, Texture2D> textures360 = new Dictionary<GeoCoordinate.Coordinate, Texture2D>();
    public static Dictionary<GeoCoordinate.Coordinate, Texture2D> textureBuffer = new Dictionary<GeoCoordinate.Coordinate, Texture2D>();
    public List<String> listaUrlSferiche = new List<string>();
    public GameObject referenceObject;
    public Transform poiParent;
    public List<OnlineMapsMarker> listaMarker = new List<OnlineMapsMarker>();
    public Texture2D personIcon;
    public float MaxScale;

    [Header("Config")]
    [SerializeField] int mapDefZoom = 19;
    [SerializeField] bool drawSfericheLine = false;
    [SerializeField] bool drawSfericheMarkers = false;
    [SerializeField] Texture2D iconaSfericaMarker;
    [SerializeField] Texture2D iconaSfericaMarkerStar;
    [SerializeField] Texture2D iconaSfericaMarkerEnd;
    [SerializeField] float iconaScale = 1f;
    [SerializeField] Color lineSfericheColor = new Color(1.0f, 0.37f, 0.43f);
    [Header("Debug")]
    [SerializeField] bool drawRouteLine = false;
    [SerializeField] bool instantiatePoiMarkers = false;
    [SerializeField] bool instantiateWayPointMarkers = false;
    [SerializeField] Texture2D iconaDebugWPMarker;
    [SerializeField] Texture2D iconaDebugPOIMarker;
    [SerializeField] float iconaDebugScale = 1f;
    [SerializeField] Color lineRouteColor = new Color(1.0f, 0.37f, 0.43f);

    readonly float breakDist = 2;
    readonly float minDist = 1.5f;

    //public GameObject markerPrefab;
    //public Transform markerParent;

    public static List<PanoPoint> PanoTour = new List<PanoPoint>();
    static public GeoCoordinate.Point start = new GeoCoordinate.Point();
    public List<PointData> routeSferiche = new List<PointData>();

    OnlineMapsMarkerManager onlineMapsMarker;
    OnlineMaps onlineMaps;
    CTRL_Player ctrl_Player;
    OnlineMapsUIRawImageControl onlineMapsUIRawImageControl;
    //OnlineMapsMarker playerMarker;

    private void Awake()
    {
        instance = this;
        ctrl_Player = GameObject.Find("Player").GetComponent<CTRL_Player>();
        CTRL_Player.OnNextPosition += NextLatLong;
        //JsonReader.OnReady += InstantiatePoints;
        JSInterop.OnItinerario += InizializzaPercorso;
    }

    // Start is called before the first frame update
    void Start()
    {
        var map = GameObject.Find("Map");
        onlineMapsMarker = map.GetComponent<OnlineMapsMarkerManager>();
        onlineMaps = map.GetComponent<OnlineMaps>();
        onlineMapsUIRawImageControl = map.GetComponent<OnlineMapsUIRawImageControl>();
        onlineMaps.zoom = mapDefZoom;
        //onlineMapsUIRawImageControl.AllowControllerOff();
        /*
                // All of this stuff is used to convert the string to float and serialize the class Coordinate for eac value from the array!
                string modifiedCoordinaateString = RemoveParentheses(UltimiValoriCordinate);
                Debug.Log(modifiedCoordinaateString);
                string[] coordinateArray = modifiedCoordinaateString.Split(new string[] { ", " }, StringSplitOptions.None);
                foreach (string coordinatePair in coordinateArray)
                {
                    string[] coordinateValues = coordinatePair.Split(' ');
                    double longitude= double.Parse(coordinateValues[0], CultureInfo.InvariantCulture);
                    double latitude= double.Parse(coordinateValues[1], CultureInfo.InvariantCulture);
                    //decimal decimLongitude = decimal.Parse(coordinateValues[0], CultureInfo.InvariantCulture);
                    //decimal decimLatitude = decimal.Parse(coordinateValues[1], CultureInfo.InvariantCulture);
                    //float longitude = (float)decimal.Round(decimLongitude, 6);
                    //float latitude = (float)decimal.Round(decimLatitude, 6);
                    GeoCoordinate.Coordinate coordinate = new GeoCoordinate.Coordinate(latitude, longitude);
                    coordinateList.Add(coordinate);
                    //Debug.Log(coordinate.Latitude);   
                }
        */

    }


    private void InizializzaPercorso(ItinerarioJS itinerario)
    {
        Debug.LogWarning("InizializzaPercorso");
        PanoTour.Clear();
        //coordinateList.Clear();
        textures360.Clear();
        StartCoroutine(Percorso(itinerario));
        DropDownValue.dropdownValue = InfoUser.GetLanguageIndex(itinerario.lingua); // assegnamo alla variabile globale della lingua.
        LanguageDropdown.ChangeLanguage(InfoUser.GetLanguageIndex(itinerario.lingua));
    }



    ShortInfo CheckCustomPoi(PoiJS pjs)
    {
        var cc = pjs.id_poi.Split(",");
        if (cc.Length == 2)
        {
            double dlat, dlon;
            if (double.TryParse(cc[1].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out dlat) &&
                double.TryParse(cc[0].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out dlon))
            {
                return new ShortInfo() { id = "custom", lat = dlat, lon = dlon };
            }
            else return null;
        }
        else return null;
    }

    IEnumerator Percorso(ItinerarioJS itinerario)
    {
        Debug.Log(JsonUtility.ToJson(itinerario));
        //aspetto finche api x dettaglio poi non è popolato
        Debug.Log("Waiting for Poi Details");
        yield return new WaitUntil(() => GETPointOfInterest.DownloadedInformationPois != null);
        Debug.LogWarning("Building Percorso");
        //identifico info poi del percorso
        List<ShortInfo> pois = new List<ShortInfo>();
        foreach (var p in itinerario.lista_poi)
        {
            var cpoi = CheckCustomPoi(p);
            if (cpoi != null)
            {
                pois.Add(cpoi);
            }
            else
            {
                foreach (var i in GETPointOfInterest.DownloadedInformationPois.infos)
                {
                    if (p.id_poi == i.id && i.lat != 0)
                    {
                        pois.Add(i);
                        //Debug.Log(i.ToString());
                        break;
                    }
                }
            }
        }
        Debug.Log($"Percorso poi: {itinerario.lista_poi.Count}, trovati: {pois.Count}");
        if (pois.Count < 2)
            yield break;
        if (instantiatePoiMarkers)
        {
            foreach (var p in pois)
            {
                var m = OnlineMapsMarkerManager.CreateItem(p.lon, p.lat, iconaDebugPOIMarker, "POI");
                m.scale = iconaDebugScale;
            }
        }
        //creo percorso da route service con infopoi
        List<GeoCoordinate.Coordinate> route = null;
        List<GeoCoordinate.Coordinate> waypois = new List<GeoCoordinate.Coordinate>();
        foreach (var p in pois)
        {
            //Debug.Log(p.ToString());
            waypois.Add(p.ToCoordinate());
        };
        RouteManager.EvaluateRoute(waypois, (coords) =>
        {
            route = coords;
        });
        Debug.Log("Wait For Route");
        //aspetto che abbia ottenut oun percorso valido
        yield return new WaitUntil(() => route != null);
        Debug.LogWarning("Route WP: " + route.Count);
        if (drawRouteLine)
        {
            Color color;
            if (!string.IsNullOrEmpty(itinerario.rgb) && ColorUtility.TryParseHtmlString(itinerario.rgb, out color)) DrawLineOnMap(route, color);
            else DrawLineOnMap(route, lineRouteColor);
        }
        //Interpolate Route Points
        List<GeoCoordinate.Coordinate> tmp_route = new List<GeoCoordinate.Coordinate>();
        for (int i = 0; i < route.Count - 1; i++)
        {
            var dist = GeoCoordinate.Utils.HaversineDistance(route[i], route[i + 1]);
            //Debug.Log(dist);
            if (dist > maxWaypointDistance)
            {
                var wp = GeoCoordinate.Utils.InterpolatePoints(route[i], route[i + 1], maxWaypointDistance, dist);
                tmp_route.AddRange(wp);
            }
            else
            {
                tmp_route.Add(route[i]);
                tmp_route.Add(route[i + 1]);
            }
        }
        route = tmp_route;
        Debug.Log("Interpolated Route Waypoints: " + route.Count);
        if (instantiateWayPointMarkers)
        {
            foreach (var w in route)
            {
                var m = OnlineMapsMarkerManager.CreateItem(w.Longitude, w.Latitude, iconaDebugWPMarker, "WP");
                m.scale = iconaDebugScale;
            }
        }
        //
        Debug.Log("Wait For Sferiche");
        //aspetto di aver scaricato le sferiche
        yield return new WaitUntil(() => JsonReader.listaSferiche != null);
        //cerco sferiche più vicine a waypoint
        foreach (var w in route)
        {
            float areaDist = maxWaypointDistance * 2;
            float minDist = areaDist;
            PointData sfericaCoordinate = null;
            foreach (var s in JsonReader.listaSferiche.sferiche)
            {
                var dist = (float)GeoCoordinate.Utils.HaversineDistance(w, s.ToCoordinate());
                //Debug.Log($"R {w.ToString()} | S {s.ToString()} -> {dist} m");
                //yield return new WaitForEndOfFrame();
                if (dist < areaDist && dist < minDist)
                {
                    minDist = dist;
                    sfericaCoordinate = s;
                    if (dist < breakDist)
                        break;
                }
            }
            //evito doppioni 
            if (sfericaCoordinate != null && !routeSferiche.Contains(sfericaCoordinate))
            {
                routeSferiche.Add(sfericaCoordinate);
            }
        }
        //pulisco vicine
        List<PointData> tmp = new List<PointData>();
        foreach (var s in routeSferiche)
        {
            bool insert = true;
            foreach (var ss in routeSferiche)
            {
                var dist = GeoCoordinate.Utils.HaversineDistance(s.ToCoordinate(), ss.ToCoordinate());
                if (dist < minDist && s.Id != ss.Id)
                {
                    insert = false;
                    break;
                }
            }
            if (insert)
                tmp.Add(s);
        }
        routeSferiche = tmp;
        //assegno coordinateList
        //coordinateList = routeSferiche;
        //Creo panotoru
        foreach (var c in routeSferiche)
        {
            var pt = new PanoPoint()
            {
                coordinate = c.ToCoordinate(),
                pointData = c
            };
            PanoTour.Add(pt);
        }
        Debug.LogWarning($"Route Waypoints: {route.Count} - sferiche: {routeSferiche.Count} - panotour: {PanoTour.Count}");
        //GENERO PUNTI
        InstantiatePoints();
        //faccio partire generazione percorso e download imamgini
        OnPercorso?.Invoke();
    }


    // use this method to instanciate the gameobjects!
    void InstantiatePoints()
    {
        Debug.Log("Instantiate POI  & Sferiche");
        if (drawSfericheLine)
            DrawLineOnMap(routeSferiche, lineSfericheColor);
        if (drawSfericheMarkers)
            InstantiateMarkers(PanoTour);
        if (PanoTour.Count < 2) return;
        //
        start = new GeoCoordinate.Point();
        for (int i = 0; i < PanoTour.Count; i++)
        {
            GeoCoordinate.Coordinate coordinate = PanoTour[i].coordinate;
            GameObject pointObject = Instantiate(referenceObject, poiParent);
            PointSkip pskp = pointObject.GetComponentInChildren<PointSkip>();
            if (pskp != null)
                pskp.index = i;
            var position = GeoCoordinate.Utils.GeoToUtm(coordinate);
            if (i == 0)
            {
                start = position;
            }
            var newPosition = position - start;
            pointObject.transform.position = newPosition.ToVector3();
            PuntiList.Add(pointObject);
        }
        //NextLatLong();
    }

    void DrawLineOnMap(List<GeoCoordinate.Coordinate> coordiante, Color colore)
    {
        List<Vector2> line = new List<Vector2>();
        for (int i = 0; i <= coordiante.Count - 1; i++)
        {
            Vector2 vector2 = new Vector2((float)coordiante[i].Longitude, (float)coordiante[i].Latitude);
            line.Add(vector2);
        }
        //Color color = new Color(1.0f, 0.37f, 0.43f);
        OnlineMapsDrawingElementManager.AddItem(new OnlineMapsDrawingLine(line, colore, 3));
    }
    void DrawLineOnMap(List<PointData> coordiante, Color colore)
    {
        List<Vector2> line = new List<Vector2>();
        for (int i = 0; i <= coordiante.Count - 1; i++)
        {
            Vector2 vector2 = new Vector2((float)coordiante[i].ToCoordinate().Longitude, (float)coordiante[i].ToCoordinate().Latitude);
            line.Add(vector2);
        }
        //Color color = new Color(1.0f, 0.37f, 0.43f);
        OnlineMapsDrawingElementManager.AddItem(new OnlineMapsDrawingLine(line, colore, 3));
    }


    // creiamo i marker in base alla mia lista di punti 

    public void InstantiateMarkers(List<PanoPoint> list)
    {
        Debug.Log("InstantateMarkers");
        int i = 0;
        foreach (var p in list)
        {
            //GameObject markerObject = Instantiate(OnlineMapsMarkerManager. markerPrefab, markerParent);
            OnlineMapsMarker marker;
            if (i == 0 || i == list.Count - 1)
            {
                marker = OnlineMapsMarkerManager.CreateItem(p.coordinate.Longitude, p.coordinate.Latitude, i == 0 ? iconaSfericaMarkerStar : iconaSfericaMarkerEnd, i.ToString());
                marker.tags.Add(i == 0 ? "start" : "end");
            }
            else
            {
                marker = OnlineMapsMarkerManager.CreateItem(p.coordinate.Longitude, p.coordinate.Latitude, iconaSfericaMarker, i.ToString());
            }
            marker.scale = iconaScale;
            marker.tags.Add("route");
            i++;
        }
        foreach (OnlineMapsMarker marker in OnlineMapsMarkerManager.instance)
        {
            marker.OnClick += OnMarkerClick;
            listaMarker.Add(marker);
        }
        //cercare con tag PLAYER
        //OnlineMapsMarkerManager.instance.items[0].texture = personIcon;
        //OnlineMapsMarkerManager.instance.items[0].scale = MaxScale;
        //playerMarker.texture = personIcon;
        //playerMarker.scale = MaxScale;
    }

    private void OnMarkerClick(OnlineMapsMarkerBase marker)
    {
        //if (!ctrl_Player.canJump) return;
        // Show in console marker label.
        if (marker.tags.Contains("route"))
        {
            ctrl_Player.SetPosition(int.Parse(marker.label), () =>
            {
                DefaultIcons();
                var m2 = (OnlineMapsMarker)marker;
                m2.texture = personIcon;
                m2.scale = MaxScale;
            });
        }
    }
    public static void OnMarkerClick(int index)
    {
        if (index < 0) return;
        Debug.LogWarning("Skip on " + index);
        instance.ctrl_Player.SetPosition(index, () =>
        {
            instance.DefaultIcons();
            var m2 = OnlineMapsMarkerManager.instance.items.Where(x => x.label == index.ToString()).FirstOrDefault();
            if (m2 != null)
            {
                m2.texture = instance.personIcon;
                m2.scale = instance.MaxScale;
            }
        });
    }

    private void NextLatLong()
    {
        Debug.Log("nextlatlong: " + CTRL_Player.posizioneAttuale);
        //playerMarker.position = coordinateList[CTRL_Player.posizioneAttuale].ToVector2();
        // playerMarker.rotationDegree = 0; //rotazione sferica
        if (ctrl_Player.setMapPosition)
        {
            //onlineMaps.latitude = PanoTour[CTRL_Player.posizioneAttuale].coordinate.Latitude;
            //onlineMaps.longitude = PanoTour[CTRL_Player.posizioneAttuale].coordinate.Longitude;
            onlineMaps.SetPositionAndZoom(PanoTour[CTRL_Player.posizioneAttuale].coordinate.Longitude, PanoTour[CTRL_Player.posizioneAttuale].coordinate.Latitude, mapDefZoom);
        }
        DefaultIcons();
        var marker = OnlineMapsMarkerManager.instance.items.Where(m => m.label == CTRL_Player.posizioneAttuale.ToString() && m.tags.Contains("route")).FirstOrDefault();
        if (marker == null)
        {
            Debug.LogError("NO ROUTE MARKER");
            return;
        }
        marker.texture = personIcon;
        marker.scale = MaxScale;
        //OnlineMapsMarkerManager.instance.items[CTRL_Player.posizioneAttuale].texture = personIcon;
        //OnlineMapsMarkerManager.instance.items[CTRL_Player.posizioneAttuale].scale = MaxScale;
    }

    public void SetMapPositionToPlayer()
    {
        Debug.Log("Set Map Position");
        ctrl_Player.setMapPosition = true;
        onlineMaps.SetPositionAndZoom(PanoTour[CTRL_Player.posizioneAttuale].coordinate.Longitude, PanoTour[CTRL_Player.posizioneAttuale].coordinate.Latitude, mapDefZoom);
    }

    public void DefaultIcons()
    {
        foreach (OnlineMapsMarker marker in OnlineMapsMarkerManager.instance.items)
        {
            if (marker.tags.Contains("route"))
            {
                marker.texture = iconaSfericaMarker;
                marker.scale = iconaScale;
            }
            if (marker.tags.Contains("start"))
            {
                marker.texture = iconaSfericaMarkerStar;
                marker.scale = iconaScale;
            }
            if (marker.tags.Contains("end"))
            {
                marker.texture = iconaSfericaMarkerEnd;
                marker.scale = iconaScale;
            }
        }
    }


}
