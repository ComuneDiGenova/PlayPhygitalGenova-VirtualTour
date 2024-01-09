using UnityEngine;

public class TEST_API_POI : MonoBehaviour
{
    public ShortInfo shortInfoPoi;
    public Info infoPOI;

    public void Inizialize(ShortInfo shortInfo)
    {
        shortInfoPoi = shortInfo;
    }
    public void GetEXtendedInfo()
    {
        StartCoroutine(GETPointOfInterest.GETExtendedInfo(shortInfoPoi, (info) =>
        {
            infoPOI = info;
            Debug.Log(infoPOI.ToString());
        }));
    }
}
