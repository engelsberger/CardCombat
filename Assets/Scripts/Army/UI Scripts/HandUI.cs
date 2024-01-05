using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{ 
    public GameObject unitPrefab;


    public void SetHandUI(List<Unit> hand)
    {
        //Clear old cards
        foreach(Transform old in transform) { Destroy(old.gameObject); }

        if (hand == null) return;

        foreach(Unit unit in hand)
        {
            GameObject unitUI = Instantiate(unitPrefab, transform);
            unitUI.GetComponent<UnitController>().SetUnit(unit);
        }
    }

    public void EnableUnitRaycasting(bool enable)
    {
        foreach(Transform unit in transform)
        {
            unit.GetComponent<Image>().raycastTarget = enable;
        }
    }
}
