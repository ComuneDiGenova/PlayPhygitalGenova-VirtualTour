using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RouteManager
{


    const string ors_proxy = "";

    class Route {
        public string type;
        //object metadata;
        public RouteFeature[] features;
        //object bbox;
    }
    class RouteFeature {
        public string type;
        public RouteGeometry geometry;
        //object bbox;
        //object properties;
    }
    class RouteGeometry {
        public string type;
        public List<List<double>> coordinates;
    }

    public static async void EvaluateRoute(List<GeoCoordinate.Coordinate> wayPoints, Action<List<GeoCoordinate.Coordinate>> callBack)
    {
        List<GeoCoordinate.Coordinate> fullRoute = new List<GeoCoordinate.Coordinate>();
        for (int i = 0; i < wayPoints.Count - 1; i++)
        {
            var start = wayPoints[i];
            var end = wayPoints[i + 1];
            var segment = await EvaluateRoute(start, end, null);
            if (segment != null)
                fullRoute.AddRange(segment);
        }
        callBack?.Invoke(fullRoute);
    }

    public static async Task<List<GeoCoordinate.Coordinate>> EvaluateRoute(GeoCoordinate.Coordinate start, GeoCoordinate.Coordinate end, Action<List<GeoCoordinate.Coordinate>> callBack)
    {
        string json = await OpenRoute(start, end);
        if (!string.IsNullOrEmpty(json))
        { /*
            var cc = json.Split("geometry");
            var cclist = "{\"coordinates" + cc[1].Split("coordinates")[1].Split("}]}")[0];
            CoordinateRoute coordianteRoute = JsonConvert.DeserializeObject<CoordinateRoute>(cclist);
            Debug.Log(JsonConvert.SerializeObject(coordianteRoute));
            */
            var route = JsonConvert.DeserializeObject<Route>(json);
            RouteGeometry coordinateRoute = route.features[0].geometry;
            Debug.Log(JsonConvert.SerializeObject(coordinateRoute));
            //
            List<GeoCoordinate.Coordinate> coordinates = new List<GeoCoordinate.Coordinate>();
            if (coordinateRoute.coordinates != null)
                Debug.Log("Raoute WP: " + coordinateRoute.coordinates.Count);
            else
                Debug.LogWarning("coordiantes null");
            if (coordinateRoute != null && coordinateRoute.coordinates != null)
            {
                foreach (var g in coordinateRoute.coordinates)
                {
                    var c = new GeoCoordinate.Coordinate((float)g[1], (float)g[0]);  //list
                    coordinates.Add(c);
                }
                callBack?.Invoke(coordinates);
                return coordinates;
            }
            else
            {
                callBack?.Invoke(null);
                return null;
            }
        }
        callBack?.Invoke(null);
        return null;
    }

    static async Task<string> OpenRoute(GeoCoordinate.Coordinate start, GeoCoordinate.Coordinate end)
    {
        //example call
        //
        WebProxy.CheckBaseUrl();
        //string url = $"{openRouteServiceUrl}?api_key={apiKey}&start={start.Longitude.ToString(CultureInfo.InvariantCulture)},{start.Latitude.ToString(CultureInfo.InvariantCulture)}&end={end.Longitude.ToString(CultureInfo.InvariantCulture)},{end.Latitude.ToString(CultureInfo.InvariantCulture)}";
        //destring url = $"{WebProxy.baseUrl}{ors_proxy}?start={start.Longitude.ToString(CultureInfo.InvariantCulture)},{start.Latitude.ToString(CultureInfo.InvariantCulture)}&end={end.Longitude.ToString(CultureInfo.InvariantCulture)},{end.Latitude.ToString(CultureInfo.InvariantCulture)}";
        string url = $"{ors_proxy}?start={start.Longitude.ToString(CultureInfo.InvariantCulture)},{start.Latitude.ToString(CultureInfo.InvariantCulture)}&end={end.Longitude.ToString(CultureInfo.InvariantCulture)},{end.Latitude.ToString(CultureInfo.InvariantCulture)}";
        Debug.Log(url);
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            uwr.certificateHandler = new AcceptAnyCertificate();
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"{url} | {uwr.result} | {uwr.error}");
                Debug.LogError(uwr.downloadHandler.text);
                return null;
            }
            else
            {
                string json = uwr.downloadHandler.text;
                Debug.Log(json);
                return json;
            }
        }
    }
}


[Serializable]
public class CoordinateRoute
{
    public List<List<double>> coordinates;
    public string type;

}
