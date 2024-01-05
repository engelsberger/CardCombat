using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimateUnit
{
    public Speed currentSpeed;
    public List<Unit> attacker;
    public List<Unit> defenders;
    public List<Unit> buffedTeammates;
    public bool attackerIsPlayer;
    public bool onlyUpdateHP = false;

    public AnimateUnit(Speed currentSpeed, List<Unit> attacker, Unit defender, bool attackerIsPlayer)
    {
        this.currentSpeed = currentSpeed;
        this.attacker = attacker;
        defenders = new List<Unit>();
        defenders.Add(defender);
        this.attackerIsPlayer = attackerIsPlayer;
    }

    public AnimateUnit(Speed currentSpeed, List<Unit> attacker, List<Unit> defenders, bool attackerIsPlayer)
    {
        this.currentSpeed = currentSpeed;
        this.attacker = attacker;
        this.defenders = defenders;
        this.attackerIsPlayer = attackerIsPlayer;
    }

    public AnimateUnit(Speed currentSpeed, List<Unit> attacker, bool attackerIsPlayer, List<Unit> buffedTeammates)
    {
        this.currentSpeed = currentSpeed;
        this.attacker = attacker;
        this.buffedTeammates = buffedTeammates;
        this.attackerIsPlayer = attackerIsPlayer;
    }

    public AnimateUnit(List<Unit> army, List<Unit> enemyArmy, bool onlyUpdateHP)
    {
        attacker = army;
        defenders = enemyArmy;
        this.onlyUpdateHP = onlyUpdateHP;
    }
}

public class AnimationController : MonoBehaviour
{
    private ArmyUI armyUI;
    private ArmyUI enemyUI;
    private TurnIndicatorUI turnIndicatorUI;

    private static float delayBetweenMarkingAttackerAndDefender = 0.2f;
    private static float delayBetweenMarkingAndAttack = 0.3f;
    private static float delayBetweenAttackAndResetColor = 0.5f;
    private static float delayBetweenAttacks = 0.3f;
    private static float delayBetweenDifferentSpeeds = 1.5f;


    public void AssignVariables(ArmyUI armyUI, ArmyUI enemyUI, TurnIndicatorUI turnIndicatorUI)
    {
        this.armyUI = armyUI;
        this.enemyUI = enemyUI;
        this.turnIndicatorUI = turnIndicatorUI;
    }

    public void TerminateAnimations()
    {
        StopAllCoroutines();
    }

    public IEnumerator AnimateAttackRound(List<AnimateUnit> units)
    {
        int unitsDone = 0;

        for(int i = 1; i < 4; i++)
        {
            List<int> armyUnitsDead = new List<int>();
            List<int> enemyUnitsDead = new List<int>();
            bool skipSpeed = true;
            bool turnChange = false;

            turnIndicatorUI.SetTurnIndicator(true, (Speed)i);

            for (int j = unitsDone; j < units.Count; j++)
            {
                if (units[j].onlyUpdateHP)
                {
                    armyUI.SetUnitHealth(units[j].attacker);
                    enemyUI.SetUnitHealth(units[j].defenders);
                }

                if (units[j].currentSpeed != (Speed)i) break;
                skipSpeed = false;

                if (!turnChange && !units[j].attackerIsPlayer) { turnIndicatorUI.SetTurnIndicator(false, (Speed)i); turnChange = true; }

                //Animate current attack

                //Set Color
                if (units[j].attackerIsPlayer)
                {
                    armyUI.SetUnitColor(units[j].attacker, Color.green);
                    yield return new WaitForSecondsRealtime(delayBetweenMarkingAttackerAndDefender);
                    if (units[j].defenders != null) enemyUI.SetUnitColor(units[j].defenders, Color.red);
                    if (units[j].buffedTeammates != null) armyUI.SetUnitColor(units[j].buffedTeammates, Color.blue);
                }
                else
                {
                    enemyUI.SetUnitColor(units[j].attacker, Color.green);
                    yield return new WaitForSecondsRealtime(delayBetweenMarkingAttackerAndDefender);
                    if (units[j].defenders != null) armyUI.SetUnitColor(units[j].defenders, Color.red);
                    if (units[j].buffedTeammates != null) enemyUI.SetUnitColor(units[j].buffedTeammates, Color.blue);
                }

                yield return new WaitForSecondsRealtime(delayBetweenMarkingAndAttack);

                //Set Health and Damage
                if (units[j].attackerIsPlayer)
                {
                    armyUI.SetUnitAmmunition(units[j].attacker);

                    if (units[j].defenders != null)
                    {
                        enemyUI.SetUnitHealth(units[j].defenders);
                        enemyUI.SetUnitDamage(units[j].defenders);
                        foreach (Unit unit in units[j].defenders) { if (unit.health <= 0) enemyUnitsDead.Add(unit.id); }
                    }
                    if (units[j].buffedTeammates != null)
                    {
                        armyUI.SetUnitHealth(units[j].buffedTeammates);
                        armyUI.SetUnitDamage(units[j].buffedTeammates);
                    }
                }
                else
                {
                    enemyUI.SetUnitAmmunition(units[j].attacker);

                    if (units[j].defenders != null)
                    {
                        armyUI.SetUnitHealth(units[j].defenders);
                        armyUI.SetUnitDamage(units[j].defenders);
                        foreach (Unit unit in units[j].defenders) { if (unit.health <= 0) armyUnitsDead.Add(unit.id); }
                    }
                    if (units[j].buffedTeammates != null)
                    {
                        enemyUI.SetUnitHealth(units[j].buffedTeammates);
                        enemyUI.SetUnitDamage(units[j].buffedTeammates);
                    }
                }

                yield return new WaitForSecondsRealtime(delayBetweenAttackAndResetColor);

                //Reset Colors
                if (units[j].attackerIsPlayer)
                {
                    armyUI.SetUnitColor(units[j].attacker, Color.white);
                    armyUI.SetSpecialSkill(units[j].attacker);
                    if (units[j].defenders != null) enemyUI.SetUnitColor(units[j].defenders, Color.white);
                    if (units[j].buffedTeammates != null) armyUI.SetUnitColor(units[j].buffedTeammates, Color.white);
                }
                else
                {
                    enemyUI.SetUnitColor(units[j].attacker, Color.white);
                    enemyUI.SetSpecialSkill(units[j].attacker);
                    if (units[j].defenders != null) armyUI.SetUnitColor(units[j].defenders, Color.white);
                    if (units[j].buffedTeammates != null) enemyUI.SetUnitColor(units[j].buffedTeammates, Color.white);
                }

                yield return new WaitForSecondsRealtime(delayBetweenAttacks);

                unitsDone++;
            }

            //Animate removing all dead Units
            armyUI.RemoveDeadUnits(armyUnitsDead);
            enemyUI.RemoveDeadUnits(enemyUnitsDead);

            if(!skipSpeed) yield return new WaitForSecondsRealtime(delayBetweenDifferentSpeeds);
        }

        turnIndicatorUI.SetTurnIndicator(false);
        GameController.instance.AttackAnimationDone();
    }
}
