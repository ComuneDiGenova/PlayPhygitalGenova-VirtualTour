using UnityEngine;
using UnityEngine.UI;

public class LegendaButton : MonoBehaviour
{
    private Button button;
    private bool chiuso = false;


    // Start is called before the first frame update
    void Start()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() => ApriChiudi());
    }

    private void ApriChiudi()
    {
        if (chiuso == false)
        {
            gameObject.GetComponent<Animator>().Play("Legenda_Open");
            chiuso = true;
        }
        else
        {
            gameObject.GetComponent<Animator>().Play("Legenda_Close");
            chiuso = false;
        }
    }

}
