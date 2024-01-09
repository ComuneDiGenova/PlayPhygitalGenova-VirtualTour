using UnityEngine;

public class CollisionTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "TRIG1")
        {
            print("enter");
        }

    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "TRIG1")
        {
            print("exit");
        }
    }
}
