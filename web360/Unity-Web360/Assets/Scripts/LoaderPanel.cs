using UnityEngine;

public class LoaderPanel : MonoBehaviour
{
    [SerializeField] private GameObject CTN_Loading;

    private void Start()
    {
        CTN_Loading.SetActive(true);
        CTRL_Player.OnStart += () =>
        {
            CTN_Loading.SetActive(false);
        };
    }
}
