using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Speed
{
    unassigned,
    fast,
    normal,
    slow
}

public enum Type
{
    unassigned,
    firstRow,
    secondRow,
    ranged,
    siege,
    multiple,
    leader
}

public enum Skill
{
    none,
    attackWeakestUnits,
    snipeWeakestUnit,
    reduceEnemyDamage,
    buffAlliedRow,
    healWeakestUnitInRow,
    increasedDamageToStructures
}

[CreateAssetMenu(fileName = "New Unit", menuName = "Unit")]
public class UnitSO : ScriptableObject
{
    public new string name;
    public string description = "Change in FileController";
    public Sprite sprite;

    public int maxHealth;
    public int maxDamage;
    public Type type = Type.firstRow;
    public Speed speed = Speed.normal;
    public Skill specialSkill = Skill.none;
    public int specialSkillValue;
    public int ammunition = -1;
    public bool isStructure = false; //Structures will be placed immediately and are not removed at the end of a battle
}
