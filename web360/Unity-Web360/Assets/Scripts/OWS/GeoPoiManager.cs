using System.Collections.Generic;
using UnityEngine;

public class GeoPoiManager : MonoBehaviour
{
    public delegate void GeoPoiEvent(List<NTPoi> ntlist);
    public static event GeoPoiEvent OnNTPoiDownloaded;

    [SerializeField] bool IstanziaSuMappa = true;
    [SerializeField] float raggioCreazione = 1000f;
    [SerializeField] float markerScale = 0.8f;
    [SerializeField] bool Bagni = true;
    [SerializeField] bool BikeSharing = true;
    [SerializeField] bool Autobus = true;
    [SerializeField] Texture2D icona_default;
    [SerializeField] Texture2D icona_bagni;
    [SerializeField] Texture2D icona_bagniAcc;
    [SerializeField] Texture2D icona_bike;
    [SerializeField] Texture2D icona_bus;

    [Header("Debug")]
    public List<NTPoi> DownloadedNTPois = new List<NTPoi>();

    private readonly GeoCoordinate.Coordinate centerCoordinate = new GeoCoordinate.Coordinate(44.409667535372584d, 8.931037781533522d);

    private void Start()
    {
        GetNTPoi();
    }

    async void GetNTPoi()
    {
        Debug.Log("Download Poi Non turistici");
        DownloadedNTPois.Clear();
        if (Bagni)
        {
            await OWS.OWSManager.GetOWSFeatures(OWS.FeatureType.Bagni, (list) =>
            {
                DownloadedNTPois.AddRange(list);
            });
        }
        if (Autobus)
        {
            await OWS.OWSManager.GetOWSFeatures(OWS.FeatureType.Autobus, (list) =>
            {
                DownloadedNTPois.AddRange(list);
            });
        }
        if (BikeSharing)
        {
            await OWS.OWSManager.GetOWSFeatures(OWS.FeatureType.BikeSharing, (list) =>
            {
                DownloadedNTPois.AddRange(list);
            });
        }
        OnNTPoiDownloaded?.Invoke(DownloadedNTPois);
        if (IstanziaSuMappa)
            InstantiateOnMap();
    }

    void InstantiateOnMap()
    {
        foreach (var p in DownloadedNTPois)
        {
            var dist = GeoCoordinate.Utils.HaversineDistance(p.coordinate, centerCoordinate);
            //Debug.Log(dist);
            if (dist > raggioCreazione) continue;
            Texture2D icon;
            switch (p.type)
            {
                case OWS.FeatureType.Bagni:
                    icon = p.details.accessibile ? icona_bagniAcc : icona_bagni;
                    break;
                case OWS.FeatureType.BikeSharing:
                    icon = icona_bike;
                    break;
                case OWS.FeatureType.Autobus:
                    icon = icona_bus;
                    break;
                default:
                    icon = icona_default;
                    break;
            }
            var marker = OnlineMapsMarkerManager.CreateItem(p.coordinate.Longitude, p.coordinate.Latitude, icon, p.id.ToString());
            marker.scale = markerScale;
            marker.tags.Add("NonTuristico");
            marker.tags.Add(p.type.ToString());
        }
    }
}
