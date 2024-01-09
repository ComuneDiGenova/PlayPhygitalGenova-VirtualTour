using UnityEngine;


public enum Languages { IT = 0, EN = 1, FR = 2, DE = 3, ES = 4, RU = 5 }

public delegate void VoidEventhandler();

[System.Serializable]
public class PanoPoint
{

    public GeoCoordinate.Coordinate coordinate;
    public PointData pointData;

    public ShortInfo associatedPoi;

    public Texture2D lowResTexture;
    public Texture2D hiResTexure;

    public override string ToString() => $"PanoPoint: {(pointData != null ? pointData.Id : "null")} || {(coordinate != null ? coordinate.ToString() : "null")} || {(associatedPoi != null ? associatedPoi.ToString() : "null")}";

}