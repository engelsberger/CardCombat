using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckUI : MonoBehaviour
{
    public GameObject unitPrefab;
    public Transform cardParent;
    public Transform deckTable;
    public Button btn_close;

    private static int maxCardsPerRow = 10;


    private void Awake()
    {
        if (cardParent == null || cardParent.GetComponent<GridLayoutGroup>() == null) Debug.Log("There is a problem with the cardParent Transform!");

        btn_close.onClick.AddListener(CloseDeskUI);
    }

    public void SetDeckUI(List<Unit> deck)
    {
        //Clear old cards
        foreach (Transform old in cardParent) { Destroy(old.gameObject); }

        if (deck == null) return;

        foreach (Unit unit in deck)
        {
            GameObject unitUI = Instantiate(unitPrefab, cardParent);
            unitUI.GetComponent<UnitController>().SetUnit(unit);
        }

        GridLayoutGroup grid = cardParent.GetComponent<GridLayoutGroup>();
        Vector2 deckSize = Vector2.zero;

        deckSize.x += grid.padding.left + grid.padding.right;
        deckSize.y += grid.padding.top + grid.padding.bottom + 50f;

        int columns = deck.Count >= maxCardsPerRow ? maxCardsPerRow : deck.Count;
        int rows = (deck.Count / maxCardsPerRow) + 1;

        deckSize.x += (grid.cellSize.x * columns) + (grid.spacing.x * (columns - 1));
        deckSize.y += (grid.cellSize.y * rows) + (grid.spacing.y * (rows - 1));

        deckTable.GetComponent<RectTransform>().sizeDelta = deckSize;
        deckTable.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    private void CloseDeskUI()
    {
        gameObject.SetActive(false);
    }
}
