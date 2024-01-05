using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameUIController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameController gameController;
    private FileController files;
    public GameObject unitPrefab;
    public GameObject unitDetailsPrefab;
    public GameObject mouseOverTextPrefab;

    private GameObject draggingUnit;
    private Unit draggedUnit;
    private GameObject unitDetailsObject;
    private GameObject mouseOverTextObject;
    private Transform source;
    private bool cardMovement = false;
    private bool dragAllowed = true;
    private bool selectCard = false;
    


    private void Awake()
    {
        
    }

    private void Update()
    {
        
    }


    public void AssignInstance()
    {
        gameController = GameController.instance;
        files = FileController.instance;
    }

    public void ListenForCardMovement(bool enable)
    {
        cardMovement = enable;
    }

    public void ListenForCardSelection(bool enable)
    {
        selectCard = enable;
    }


    #region PointerEventHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.pointerEnter == null)
        {
            Debug.LogError("PointerEnter is Null");
            return;
        }

        GameObject selected = eventData.pointerEnter;

        Destroy(mouseOverTextObject);
        if(!selected.CompareTag(files.questionmarkTag) && !selected.CompareTag(files.unitDetailTag)) Destroy(unitDetailsObject);

        if (selected.CompareTag(files.questionmarkTag))
        {
            mouseOverTextObject = Instantiate(mouseOverTextPrefab, Input.mousePosition, transform.rotation, transform);

            RectTransform rTransform = mouseOverTextObject.GetComponent<RectTransform>();
            rTransform.anchoredPosition = new Vector2(rTransform.anchoredPosition.x + 100f, rTransform.anchoredPosition.y);
            rTransform.anchoredPosition = AdjustObjectToScreenSize(rTransform.anchoredPosition, rTransform.rect.size);

            mouseOverTextObject.GetComponent<MouseOverTextUI>().SetMouseOverText(selected.GetComponent<MouseOverUIData>().GetData());
        }
        else if(selected.CompareTag(files.unitTag))
        {
            unitDetailsObject = Instantiate(unitDetailsPrefab, Input.mousePosition, transform.rotation, transform);

            RectTransform rTransform = unitDetailsObject.GetComponent<RectTransform>();
            rTransform.anchoredPosition = AdjustObjectToScreenSize(rTransform.anchoredPosition, rTransform.rect.size);

            unitDetailsObject.GetComponent<UnitDetailsUI>().SetUnitDetails(selected.GetComponent<UnitController>().unit, selectCard);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!cardMovement || selectCard) return;

        if (eventData.pointerEnter.tag == files.unitTag)
        {
            draggedUnit = eventData.pointerEnter.GetComponent<UnitController>().unit;

            if (draggedUnit.isLocked)
            { 
                draggedUnit = null; 
                dragAllowed = false; 
                return; 
            }

            draggingUnit = Instantiate(unitPrefab, Input.mousePosition, transform.rotation, transform);
            draggingUnit.GetComponent<Image>().raycastTarget = false;
            draggingUnit.GetComponent<UnitController>().SetUnit(eventData.pointerEnter.GetComponent<UnitController>().unit);

            source = eventData.pointerEnter.transform.parent.name == "Hand" ? eventData.pointerEnter.transform.parent : eventData.pointerEnter.transform.parent.parent;
            gameController.PlayerMovesCard(draggedUnit, source);

            dragAllowed = true;
        }
        else dragAllowed = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!cardMovement || !dragAllowed) return;

        draggingUnit.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!cardMovement || !dragAllowed) return;

        gameController.PlayerMovedCard(draggedUnit, source, eventData.pointerEnter.transform);

        Destroy(draggingUnit);
        draggedUnit = null;
        source = null;
        dragAllowed = false;
    }
    #endregion

    private Vector2 AdjustObjectToScreenSize(Vector2 currentPos, Vector2 size) //returns new Position of an object in relation to screen size
    {
        Vector2 screenSize = GetComponent<RectTransform>().rect.size;
        Vector2 newPos = new Vector2(currentPos.x, currentPos.y);

        if (currentPos.x + (size.x / 2f) > screenSize.x / 2f) newPos.x = (screenSize.x / 2f) - (size.x / 2f);
        else if (currentPos.x - (size.x / 2f) < screenSize.x / -2f) newPos.x = (screenSize.x / -2f) + (size.x / 2f);

        if (currentPos.y + (size.y / 2f) > screenSize.y / 2f) newPos.y = (screenSize.y / 2f) - (size.y / 2f);
        else if (currentPos.y - (size.y / 2f) < screenSize.y / -2f) newPos.y = (screenSize.y / -2f) + (size.y / 2f);

        return newPos;
    }
}
