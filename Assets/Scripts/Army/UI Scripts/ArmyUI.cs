using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyUI : MonoBehaviour
{
    private FileController files;
    public GameObject unitPrefab;
    public Transform firstRow;
    public Transform secondRow;
    public Transform ranged;
    public Transform siege;
    public Transform leader;
    public Image heart1;
    public Image heart2;


    public void SetCompleteArmyUI(List<Unit> army)
    {
        files = FileController.instance;

        //Remove old objects
        foreach (Transform batallion in transform)
        {
            foreach (Transform old in batallion) { if(old.CompareTag(files.unitTag)) Destroy(old.gameObject); }
        }

        if (army == null) return;

        for(int i = 0; i < army.Count; i++)
        {
            Transform parent;
            switch (army[i].setType)
            {
                case Type.firstRow:
                    parent = firstRow;
                    break;
                case Type.secondRow:
                    parent = secondRow;
                    break;
                case Type.ranged:
                    parent = ranged;
                    break;
                case Type.siege:
                    parent = siege;
                    break;
                case Type.leader:
                    parent = leader;
                    break;
                default:
                    Debug.LogError("Unit Type not assigned!");
                    parent = firstRow;
                    break;
            }

            GameObject unitUI = Instantiate(unitPrefab, parent);
            unitUI.GetComponent<UnitController>().SetUnit(army[i]);
        }
    }

    public void SetLeaderHealth(int health) //Displays score basically, can be either 0, 1 or 2
    {
        switch (health)
        {
            case 0:
                heart1.sprite = FileController.instance.leaderHeartEmpty;
                heart2.sprite = FileController.instance.leaderHeartEmpty;
                break;
            case 1:
                heart1.sprite = FileController.instance.leaderHeartFull;
                heart2.sprite = FileController.instance.leaderHeartEmpty;
                break;
            case 2:
                heart1.sprite = FileController.instance.leaderHeartFull;
                heart2.sprite = FileController.instance.leaderHeartFull;
                break;
            default:
                Debug.LogError("Leader Health / Score outside of bounds (0-2)!");
                break;
        }
    }

    public bool CheckIfRowIsFull(Type type, int maxCount)
    {
        int unitCount;
        switch (type)
        {
            case Type.firstRow:
                unitCount = firstRow.childCount;
                break;
            case Type.secondRow:
                unitCount = secondRow.childCount;
                break;
            case Type.ranged:
                unitCount = ranged.childCount;
                break;
            case Type.siege:
                unitCount = siege.childCount;
                break;
            default:
                return true;
        }

        return unitCount >= maxCount;
    }

    public void RemoveDeadUnits(List<int> unitIDs)
    {
        foreach (Transform batallion in transform)
        {
            foreach (Transform unit in batallion)
            {
                if (unit.CompareTag(files.unitTag) && unitIDs.Contains(unit.GetComponent<UnitController>().unit.id)) Destroy(unit.gameObject);
            }
        }
    }

    public void SetUnitColor(List<Unit> units, Color color)
    {
        foreach(Unit unit in units)
        {
            UnitController unitUI = FindUnitByID(unit.id);
            if (unitUI != null) unitUI.SetColor(color);
        }
    }

    public void SetUnitHealth(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            UnitController unitUI = FindUnitByID(unit.id);
            if (unitUI != null) unitUI.SetHealth(unit.health);
        }
    }

    public void SetUnitDamage(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            UnitController unitUI = FindUnitByID(unit.id);
            if (unitUI != null) unitUI.SetDamage(unit.damage);
        }
    }

    public void SetUnitAmmunition(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            UnitController unitUI = FindUnitByID(unit.id);
            if (unitUI != null) unitUI.SetAmmunition(unit.ammunition);
        }
    }

    public void SetSpecialSkill(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            UnitController unitUI = FindUnitByID(unit.id);
            if (unitUI != null) unitUI.ActivateSpecialSkill(unit.skillActive);
        }
    }

    private UnitController FindUnitByID(int id)
    {
        foreach (Transform batallion in transform)
        {
            foreach (Transform unit in batallion)
            {
                if (!unit.CompareTag(files.unitTag)) continue;
                UnitController unitUI = unit.GetComponent<UnitController>();
                if (unitUI.unit.id == id) return unitUI;
            }
        }

        return null;
    }


    public void EnableUnitRaycasting(bool enable)
    {
        foreach (Transform batallion in transform)
        {
            foreach (Transform unit in batallion) { if (unit.CompareTag(files.unitTag)) unit.GetComponent<Image>().raycastTarget = enable; }
        }
    }

    public void LockUnits()
    {
        foreach (Transform batallion in transform)
        {
            foreach (Transform unit in batallion) { if (unit.CompareTag(files.unitTag)) unit.GetComponent<UnitController>().unit.Lock(true); }
        }
    }
}
