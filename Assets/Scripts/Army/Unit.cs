using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    public UnitSO unit;
    public int id;
    public int defensePriority; //The unit with the highest priority will be attacked first
    public int health; //Current Health of the Unit
    public int damage; //current damage of the unit
    public int ammunition; //used for some special abilities
    public Type setType = Type.unassigned; //To keep track where a multiple type unit is set
    public bool skillActive;
    public bool isLocked;


    public Unit(UnitSO unit, int id)
    {
        this.unit = unit;
        this.id = id;
        health = unit.maxHealth;
        damage = unit.maxDamage;
        if (unit.specialSkill == Skill.snipeWeakestUnit) damage += unit.specialSkillValue;
        ammunition = unit.ammunition;
        skillActive = true;
        isLocked = true;
        if (unit.type != Type.multiple) setType = unit.type;
        if (unit.type == Type.leader) defensePriority = 0;

        if (unit.name == "") Debug.LogError("Alert! No name given!");
        else if (unit.type == Type.unassigned || unit.speed == Speed.unassigned) Debug.LogError("Type/Speed is not assigned for " + unit.name);
        else if (unit.maxHealth <= 0 || (unit.maxDamage <= 0 && !unit.isStructure)) Debug.LogError("Health/Damage wrong for " + unit.name);
    }

    public Unit(UnitSO unit, int id, int health, int damage)
    {
        this.unit = unit;
        this.id = id;
        this.health = health;
        this.damage = damage;
        isLocked = true;

        if (unit.name == "") Debug.LogError("Alert! No name given!");
        else if (unit.type == Type.unassigned || unit.speed == Speed.unassigned) Debug.LogError("Type/Speed is not assigned for " + unit.name);
        else if (unit.maxHealth <= 0 || (unit.maxDamage <= 0 && !unit.isStructure)) Debug.LogError("Health/Damage wrong for " + unit.name);
    }

    public Unit(UnitSO unit, int id, int ammunition, bool skillActive)
    {
        this.unit = unit;
        this.id = id;
        this.ammunition = ammunition;
        this.skillActive = skillActive;
        isLocked = true;

        if (unit.name == "") Debug.LogError("Alert! No name given!");
        else if (unit.type == Type.unassigned || unit.speed == Speed.unassigned) Debug.LogError("Type/Speed is not assigned for " + unit.name);
        else if (unit.maxHealth <= 0 || (unit.maxDamage <= 0 && !unit.isStructure)) Debug.LogError("Health/Damage wrong for " + unit.name);
    }

    public int Strike()
    {
        return damage;
    }

    public int Hit(int damage) ///Returns excessive damage (returns 0 if unit is still alive)
    {
        if (health - damage <= 0)
        {
            int restDamage = damage - health; //Damage that can be carried over to the next unit in line
            health = 0;
            return restDamage;
        }

        health -= damage;
        return 0;
    }

    public void ChangeDamageByValue(int value)
    {
        if (damage + value <= 1) damage = 1;
        else damage += value;
    }

    public void ChangeHealthByValue(int value)
    {
        if (health + value <= 1) health = 1;
        else health += value;
    }

    public void RemoveTemporaryHP()
    {
        if (health > unit.maxHealth) health = unit.maxHealth;
    }

    public bool UseAmmunition() //returns false if unit is out of ammo
    {
        if (ammunition <= -1) Debug.LogError("Tried to use ammunition for a unit that has no ammunition configured!");

        if (ammunition <= 0) return false;

        ammunition--;
        return true;
    }

    public void EnableSkill(bool enable)
    {
        skillActive = enable;
    }

    public void Lock(bool enable)
    {
        isLocked = enable;
    }
}