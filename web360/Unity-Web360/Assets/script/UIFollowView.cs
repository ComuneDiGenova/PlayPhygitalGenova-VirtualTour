using UnityEngine;

public class UIFollowView : MonoBehaviour
{

    // Start is called before the first frame update
    //private void Awake()
    //{
    //    camera = 
    //}

    void Start()
    {
        //cameraVR = GameObject.Find("CenterEyeAnchor").transform;
    }

    // Update is called once per frame
    void Update()
    {

        transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position, Vector3.up);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y - 180, 0);

        //transform.LookAt(new Vector3(0f, 0f, Camera.main.transform.position.z), Vector3.up);
        //transform.SetAsFirstSibling();
    }
}
