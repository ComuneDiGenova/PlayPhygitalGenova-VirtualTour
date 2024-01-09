using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputDebug : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.LogWarning("Over GameObject: " + EventSystem.current.IsPointerOverGameObject());
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                Debug.LogWarning($"CLICK ON: {Hierarchy(EventSystem.current.currentSelectedGameObject)}");
            }
            else
            {
                Debug.LogWarning("CLICK ON NULL");
            }
            PointerEventData ped = new PointerEventData(EventSystem.current);
            ped.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);
            Debug.LogWarning("Ray HITS: " + results.Count);
            foreach (var hit in results)
            {
                Debug.LogWarning(Hierarchy(hit.gameObject));
            }
        }
    }

    string Hierarchy(GameObject target)
    {
        string tree = target.name;
        var obj = target.transform.parent;
        while (obj != null)
        {
            tree = obj.name + "/" + tree;
            obj = obj.parent;
        }
        tree = "/" + tree;
        return tree;
    }
}
