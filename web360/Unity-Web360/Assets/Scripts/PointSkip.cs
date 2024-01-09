using UnityEngine;

public class PointSkip : MonoBehaviour
{
    public int index = -1;


    private void OnMouseDown()
    {
        if (!GETPointOfInterest.dentroUnPOI)
        {
            CtrlCoordinate.OnMarkerClick(index);
        }

    }
}
