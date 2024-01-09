using System.Collections.Generic;
using UnityEngine;

public class test_route : MonoBehaviour
{
    [SerializeField] GeoCoordinate.Coordinate start;
    [SerializeField] GeoCoordinate.Coordinate end;

    public List<GeoCoordinate.Coordinate> route = new List<GeoCoordinate.Coordinate>();

    private void Start()
    {
        RouteManager.EvaluateRoute(start, end, (coords) =>
        {
            route = coords;
            Debug.Log("OK");
        });
    }
}
