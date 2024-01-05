using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    System.Random rd = new System.Random();


    public Unit Play(List<Unit> hand, List<Unit> currentArmy)
    {
        if (hand == null || hand.Count <= 0) return null;

        List<Unit> unitsToPlay = hand;
        if (currentArmy.Count == 0)
        {
            unitsToPlay = hand.Where(u => u.setType != Type.unassigned).ToList();
            if (unitsToPlay.Count <= 0) unitsToPlay = hand;
        }

        Unit unitToPlay = unitsToPlay[rd.Next(0, unitsToPlay.Count - 1)];

        if(unitToPlay.setType == Type.unassigned)
        {
            unitToPlay.setType = PlaceUnitInBestRow(unitToPlay, currentArmy);
            if (unitToPlay.setType == Type.unassigned) unitToPlay.setType = (Type)rd.Next(1, 4);
        }

        return unitToPlay;
    }


    #region Find best row for multiple-type-units
    private Type PlaceUnitInBestRow(Unit unitToPlay, List<Unit> currentArmy)
    {
        Type bestRow = Type.unassigned;

        if (unitToPlay.unit.specialSkill == Skill.healWeakestUnitInRow)
        {
            List<Unit> weakestUnits = FindWeakestUnits(currentArmy);
            Unit weakestUnit = FindUnitByHighestDefensePriority(weakestUnits);

            if (weakestUnit == null) bestRow = (Type)rd.Next(3, 4);
            else bestRow = weakestUnit.setType;
        }
        else if (unitToPlay.unit.specialSkill == Skill.buffAlliedRow)
        {
            Type targetRow = FindRowWithMostUnits(currentArmy);

            if (targetRow == Type.unassigned) bestRow = (Type)rd.Next(3, 4);
            else bestRow = targetRow;
        }
        else Debug.LogError("AI does not know where to place " + unitToPlay.unit.name);

        return bestRow;
    }

    private Type FindRowWithMostUnits(List<Unit> pool)
    {
        if (pool == null || pool.Count < 1) return Type.unassigned;

        pool = pool.Where(u => u.setType != Type.leader).ToList();
        if (pool == null || pool.Count < 1) return Type.unassigned;

        int[] rowCount = new int[4] { 0, 0, 0, 0 };

        foreach(Unit unit in pool)
        {
            rowCount[(int)(unit.setType - 1)]++;
        }

        return (Type)(rowCount.ToList().IndexOf(rowCount.Max()) + 1);
    }

    private List<Unit> FindWeakestUnits(List<Unit> pool)
    {
        if (pool == null || pool.Count < 1) return null;

        pool = pool.Where(u => u.setType != Type.leader).ToList();
        if (pool == null || pool.Count < 1) return null;

        List<Unit> lowestHealthUnits = new List<Unit>();
        lowestHealthUnits.Add(pool[0]);

        for (int i = 1; i < pool.Count; i++)
        {
            if (pool[i].health < lowestHealthUnits[0].health)
            {
                lowestHealthUnits.Clear();
                lowestHealthUnits.Add(pool[i]);
            }
            else if (pool[i].health == lowestHealthUnits[0].health)
            {
                lowestHealthUnits.Add(pool[i]);
            }
        }

        return lowestHealthUnits;
    }

    private Unit FindUnitByHighestDefensePriority(List<Unit> pool)
    {
        if (pool == null || pool.Count < 1) return null;

        Unit highestPriorityUnit = pool[0];

        for (int i = 1; i < pool.Count; i++)
        {
            if (pool[i].defensePriority > highestPriorityUnit.defensePriority) highestPriorityUnit = pool[i];
        }

        return highestPriorityUnit;
    }
    #endregion
}
