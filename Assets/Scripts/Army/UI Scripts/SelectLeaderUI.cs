using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectLeaderUI : MonoBehaviour
{
    public Transform parent;
    public RectTransform txtSelectLeader;
    public GameObject unitPrefab;


    public void DisplayLeaders(List<Unit> deck)
    {
        //Remove previous leaders
        foreach(Transform t in parent) { Destroy(t.gameObject); }

        List<Unit> leaders = deck.Where(u => u.setType == Type.leader).ToList();

        foreach (Unit unit in leaders)
        {
            GameObject unitUI = Instantiate(unitPrefab, parent);
            unitUI.GetComponent<UnitController>().SetUnit(unit);
        }

        GridLayoutGroup grid = parent.GetComponent<GridLayoutGroup>();
        Vector2 uiSize = Vector2.zero;

        uiSize.x += grid.padding.left;
        uiSize.x += grid.padding.right;
        uiSize.y += grid.padding.top;
        uiSize.y += grid.padding.bottom;

        uiSize.x += grid.cellSize.x * leaders.Count;
        uiSize.y += grid.cellSize.y;

        uiSize.x += grid.spacing.x * (parent.childCount - 1);
        parent.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(uiSize.x, uiSize.y + 20f);
        parent.GetComponent<RectTransform>().sizeDelta = uiSize;
        parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -10f);
        txtSelectLeader.anchoredPosition = new Vector2(0f, txtSelectLeader.rect.height / -2f);
    }
}
