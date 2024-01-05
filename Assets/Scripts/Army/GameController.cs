using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* DA RULES
 * 
 * Jeder legt abwechselnd eine Karte hin
 * Es ist auch erlaubt, keine Karte zu legen wenn man dran ist
 * Computer beginnt mit Kartenlegen
 * Nachdem jeder 5 mal dran war, beginnt der erste Kampf
 * Danach wird immer gekämpft, nachdem jeder 2 mal dran war
 * 
 * Wenn von einem Spieler alle Einheiten besiegt wurden, bekommt der Gegner einen Punkt
 * Wenn sich beide Armeen gleichzeitig auslöschen, bekommt keiner einen Punkt
 * Wer als erstes 2 Punkte hat, gewinnt das Spiel
 * 
 * Units, die sich nach Rundenende auf dem Spielfeld befinden, werden entfernt für die nächste Runde
 * Units, die sich in der Hand befinden nach Rundenende, dürfen behalten werden
 * 
 * Units vom selben Typ greifen gleichzeitig an, d.h. der Schaden wird multipliziert
 * 
 * Es greifen zuerst die fast Units an, dann die normal Units und zu guter letzt die slow units.
 * Units, die Buffs/debuffs verursachen, machen dies vor Rundenstart (normal speed buff unit wirft buff nach angriff der schnellen units, aber vor angriff der normalen units)
 * 
 * Units greifen den Gegner immer in dieser Reihenfolge an: Zuerst Defense, dann Fast, dann Ranged und zuletzt Siege
 * Bei Units vom gleichen Typ wird immer diejenige angegriffen, die ganz oben liegt.
 * Es sei denn, die Unit hat eine spezielle Fähigkeit, die diese Reihenfolge umgeht
 * 
 * Skill: attack weakest unit
 * Greift die Unit mit den niedrigsten max HP an (achtung: max HP, nicht aktuelle HP)
 * 
 * Skill: reduce enemy damage
 * Greift den Gegner mit dem meisten Schaden an und reduziert dessen Schaden um einen fixen Wert. Dies kann mit mehreren Angreifer mit dieser Fähigkeit stacken.
 * Die Schadensreduzierung bekommt jeder, der von dieser Unit angegriffen wird (können auch mehrere in einer Runde sein)
 */

public class GameController : MonoBehaviour
{
    public static GameController instance;

    private EnemyAI enemy;
    public GameUIController gameUI;
    private AnimationController animate;
    private FileController files;
    public ArmyUI armyUI;
    public ArmyUI enemyUI;
    public HandUI playerHandUI;
    public DeckUI playerDeckUI;
    public GameOverUI gameOverUI;
    public SelectLeaderUI selectLeaderUI;
    public TurnIndicatorUI turnIndicatorUI;
    public Button btn_fight;
    public Button btn_reset;
    public Button btn_nextRound;
    public Button btn_openDeck;

    private List<Unit> army = new List<Unit>();
    private Unit armyLeader;
    private List<Unit> armyStructures = new List<Unit>();
    private List<Unit> enemyArmy = new List<Unit>();
    private Unit enemyLeader;
    private List<Unit> enemyStructures = new List<Unit>();
    private List<Unit> playerDeck = new List<Unit>();
    private List<Unit> enemyDeck = new List<Unit>();
    private List<Unit> playerHand = new List<Unit>();
    private List<Unit> enemyHand = new List<Unit>();
    private List<AnimateUnit> animateUnits = new List<AnimateUnit>();

    private int round = 0;
    private bool yourTurn = false;
    private int roundsUntilFight = 0;
    private Unit tempCardAdded = null; //saves the card that has been added during player turn until player ends his turn
    private Vector2Int score = Vector2Int.zero;
    private bool roundOver = false;

    private static int roundsUntilBeginningFight = 10; //Everyone puts down five cards before the next fight begins
    private static int roundsUntilNextFight = 4; //After that, everyone puts down two additional cards, then the next fight begins
    private static int beginningHandCount = 8; //Every player begins with drawing 5 cards
    private static int maxHandCount = 8;
    private static int maxUnitsInRow = 4;
    public System.Random rand = new System.Random();

    #region UnitList

    #endregion


    private void Awake()
    {
        enemy = GetComponent<EnemyAI>();
        animate = GetComponent<AnimationController>();
        files = GetComponent<FileController>();

        instance = this;
        files.SetInstance();
        gameUI.AssignInstance();
        animate.AssignVariables(armyUI, enemyUI, turnIndicatorUI);
        armyUI.SetCompleteArmyUI(null);
        enemyUI.SetCompleteArmyUI(null);

        if (btn_fight == null) Debug.LogError("Button missing!");
        else btn_fight.onClick.AddListener(BtnFight);

        if (btn_nextRound == null) Debug.LogError("Button NextRound missing");
        else btn_nextRound.onClick.AddListener(BtnNextRound);

        if (btn_openDeck == null) Debug.LogError("Button Deck missing");
        else btn_openDeck.onClick.AddListener(BtnOpenDeck);

        if (btn_reset == null) Debug.LogError("Button Reset missing");
        else btn_reset.onClick.AddListener(BtnReset);

        //Disabled by default
        playerDeckUI.gameObject.SetActive(false);
        gameOverUI.gameObject.SetActive(false);
        turnIndicatorUI.SetTurnIndicator(false);
    }

    private void Start()
    {
        //Add some Cards into the deck
        AddTestingCardsToDeck();

        //Select Leader
        selectLeaderUI.DisplayLeaders(playerDeck);
        gameUI.ListenForCardSelection(true);
    }

    private void Update()
    {

    }

    #region PublicScripts
    public void PlayerMovesCard(Unit card, Transform source)
    {
        armyUI.EnableUnitRaycasting(false);
        playerHandUI.EnableUnitRaycasting(false);

        if (source.tag == files.armyTag)
        {
            army.Remove(card);
            armyUI.SetCompleteArmyUI(army);
        }
        else
        {
            playerHand.Remove(card);
            playerHandUI.SetHandUI(playerHand);
        }
    }

    public void PlayerMovedCard(Unit card, Transform source, Transform target) //called by GameUIController when player drops a card somewhere
    {
        if(source.tag != files.armyTag && target.tag == files.armyTag)
        {
            if (card.unit.type == Type.multiple)
            {
                switch (target.name)
                {
                    case "1.Row":
                        card.setType = Type.firstRow;
                        break;
                    case "2.Row":
                        card.setType = Type.secondRow;
                        break;
                    case "Ranged":
                        card.setType = Type.ranged;
                        break;
                    case "Siege":
                        card.setType = Type.siege;
                        break;
                }
            }

            if (!armyUI.CheckIfRowIsFull(card.setType, maxUnitsInRow))
            {
                if (tempCardAdded != null)
                {
                    playerHand.Add(tempCardAdded);
                    army.Remove(tempCardAdded);
                }

                tempCardAdded = card;
                AddUnitAndSetDefensePriority(card, army);
            }
            else playerHand.Add(card);
        }
        else if(source.tag == files.armyTag && target.tag != files.armyTag)
        {
            SetDefensePriority(army);
            playerHand.Add(card);
            tempCardAdded = null;
        }
        else if(source.tag == files.armyTag && target.tag == files.armyTag)
        {
            AddUnitAndSetDefensePriority(card, army);
        }
        else
        {
            playerHand.Add(card);
        }

        playerHand = playerHand.OrderBy(u => u.unit.type).ThenBy(u => u.unit.name).ToList();

        armyUI.SetCompleteArmyUI(army);
        playerHandUI.SetHandUI(playerHand);

        armyUI.EnableUnitRaycasting(true);
        playerHandUI.EnableUnitRaycasting(true);
    }

    public void FullReset()
    {
        ResetGame(true);

        btn_fight.transform.parent.gameObject.SetActive(true);
    }

    public void AttackAnimationDone()
    {
        armyUI.SetLeaderHealth(2 - score.y); //2 - enemy score
        enemyUI.SetLeaderHealth(2 - score.x); //2 - army score
        armyUI.SetCompleteArmyUI(army);
        enemyUI.SetCompleteArmyUI(enemyArmy);

        if (score.x >= 2 || score.y >= 2)
        {
            gameOverUI.gameObject.SetActive(true);
            gameOverUI.SetWinScreen(score.x >= 2 ? true : false);

            btn_fight.transform.parent.gameObject.SetActive(true);
            return;
        }

        if (roundOver)
        {
            btn_fight.transform.parent.gameObject.SetActive(true);
            return;
        }

        NextRound();
    }

    public void LeaderSelected(Unit leader)
    {
        gameUI.ListenForCardSelection(false);

        armyLeader = new Unit(leader.unit, leader.id);
        army.Add(armyLeader);

        List<Unit> otherLeaderCards = playerDeck.Where(u => u.setType == Type.leader).ToList();
        foreach(Unit u in otherLeaderCards) { playerDeck.Remove(u); }

        selectLeaderUI.gameObject.SetActive(false);

        Unit selectEnemyLeader = enemyDeck.Where(u => u.setType == Type.leader).First();
        enemyLeader = new Unit(selectEnemyLeader.unit, selectEnemyLeader.id);
        enemyArmy.Add(enemyLeader);

        List<Unit> otherEnemyLeaderCards = enemyDeck.Where(u => u.setType == Type.leader).ToList();
        foreach (Unit u in otherEnemyLeaderCards) { enemyDeck.Remove(u); }

        armyUI.SetCompleteArmyUI(army);
        enemyUI.SetCompleteArmyUI(enemyArmy);
    }
    #endregion

    #region ButtonScripts
    private void BtnOpenDeck()
    {
        playerDeck = playerDeck.OrderBy(u => u.unit.type).ThenBy(u => u.unit.name).ToList();

        playerDeckUI.gameObject.SetActive(true);
        playerDeckUI.SetDeckUI(playerDeck);
    }

    private void BtnReset()
    {
        animate.TerminateAnimations();

        FullReset();
    }

    private void BtnFight()     //Starts a fight
    {
        btn_fight.transform.parent.gameObject.SetActive(false);

        ResetGame(false);

        //Add structure Units to battleground
        for(int i = 0; i < enemyDeck.Count; i++)
        {
            if (enemyDeck[i].unit.isStructure)
            {
                AddUnitAndSetDefensePriority(enemyDeck[i], enemyArmy);
                enemyDeck.Remove(enemyDeck[i]);
                i--;
            }
        }
        for (int i = 0; i < playerDeck.Count; i++)
        {
            if (playerDeck[i].unit.isStructure)
            {
                AddUnitAndSetDefensePriority(playerDeck[i], army);
                playerDeck.Remove(playerDeck[i]);
                i--;
            }
        }

        //Draw cards to hand
        DrawCardsFromDeck(beginningHandCount - playerHand.Count, true);
        DrawCardsFromDeck(beginningHandCount - enemyHand.Count, false);

        //Order player hand
        playerHand = playerHand.OrderBy(u => u.unit.type).ThenBy(u => u.unit.name).ToList();
        playerHandUI.SetHandUI(playerHand);

        NextRound();
    }

    private void BtnNextRound() //Pressed when player is done with his round
    {
        btn_nextRound.enabled = false;
        gameUI.ListenForCardMovement(false);
        armyUI.LockUnits();
        tempCardAdded = null;

        yourTurn = !yourTurn;

        if (roundsUntilFight <= 0)
        {
            animateUnits.Clear();
            int outcome = Fight();

            if (outcome != 0)
            {
                if (outcome == 1)
                {
                    Debug.Log("You win this round!");
                    score.x++;
                }
                else if (outcome == -1)
                {
                    Debug.Log("You lose this round!");
                    score.y++;
                }
                else if (outcome == 69) Debug.Log("No winners this round - everyone died!");

                roundOver = true;
            }

            roundsUntilFight = roundsUntilNextFight;
            SetDefensePriority(army);
            SetDefensePriority(enemyArmy);

            StartCoroutine(animate.AnimateAttackRound(animateUnits));
            return;
        }

        NextRound();
    }
    #endregion

    private void NextRound()
    {
        round++;
        roundsUntilFight--;

        if(yourTurn)
        {
            if (playerHand.Count < maxHandCount && !DrawCardsFromDeck(1, true)) Debug.Log("Player has no more cards to draw");
            playerHand = playerHand.OrderBy(u => u.unit.type).ThenBy(u => u.unit.name).ToList();
            playerHandUI.SetHandUI(playerHand);

            gameUI.ListenForCardMovement(true);
            btn_nextRound.enabled = true;
        }
        else
        {
            yourTurn = !yourTurn;

            if (enemyHand.Count < maxHandCount && !DrawCardsFromDeck(1, false)) Debug.Log("Enemy has no more cards to draw");

            Unit unitPlayed = enemy.Play(enemyHand, enemyArmy);

            if(unitPlayed != null)
            {
                AddUnitAndSetDefensePriority(unitPlayed, enemyArmy);
                enemyHand.Remove(unitPlayed);
                enemyUI.SetCompleteArmyUI(enemyArmy);
            }

            NextRound();
        }
    }

    private bool DrawCardsFromDeck(int amount, bool isPlayerDeck) //returns false if deck empty
    {
        if (isPlayerDeck && playerDeck.Count <= 0) return false;
        else if (!isPlayerDeck && enemyDeck.Count <= 0) return false;

        for (int i = 0; (isPlayerDeck ? playerDeck.Count > 0 : enemyDeck.Count > 0) && i < amount; i++)
        {
            if (isPlayerDeck)
            {
                Unit card = playerDeck[rand.Next(0, playerDeck.Count)];
                playerDeck.Remove(card);
                if(card.setType != Type.leader) card.Lock(false);
                playerHand.Add(card);
            }
            else
            {
                Unit card = enemyDeck[rand.Next(0, enemyDeck.Count)];
                enemyDeck.Remove(card);
                enemyHand.Add(card);
            }
        }

        return true;
    }

    private void AddUnitAndSetDefensePriority(Unit newUnit, List<Unit> armyType)
    {
        SetDefensePriority(armyType);
        newUnit.defensePriority = 1;

        foreach(Unit unit in armyType)
        {
            if (unit.setType == Type.leader) continue;

            if(newUnit.setType == Type.siege)
            {
                unit.defensePriority++;
            }
            else if(newUnit.setType == Type.ranged)
            {
                if (unit.setType == Type.ranged || unit.setType == Type.secondRow || unit.setType == Type.firstRow) unit.defensePriority++;
                else newUnit.defensePriority++;
            }
            else if(newUnit.setType == Type.secondRow)
            {
                if (unit.setType == Type.secondRow || unit.setType == Type.firstRow) unit.defensePriority++;
                else newUnit.defensePriority++;
            }
            else if(newUnit.setType == Type.firstRow)
            {
                if (unit.setType == Type.firstRow) unit.defensePriority++;
                else newUnit.defensePriority++;
            }
        }

        armyType.Add(newUnit);
        armyType = armyType.OrderByDescending(u => u.defensePriority).ToList();
    }

    private void SetDefensePriority(List<Unit> armyType) //sets defense priority, so that all values differ by one again
    {
        armyType = armyType.OrderByDescending(u => u.defensePriority).ToList();

        for(int i = 0; i < armyType.Count; i++)
        {
            if(armyType[i].setType != Type.leader) armyType[i].defensePriority = armyType.Count - i - 1;
        }
    }

    #region Fighting
    private int Fight() //returns 1 for a win, 0 for fight is ongoing, -1 for a loss, 69 for undecided (both armies died)
    {
        int outcome = FightRound(Speed.fast);
        if (outcome == 0) outcome = FightRound(Speed.normal);
        if (outcome == 0) outcome = FightRound(Speed.slow);

        RemoveTemporaryHP(army);
        RemoveTemporaryHP(enemyArmy);

        return outcome;
    }

    private void RemoveTemporaryHP(List<Unit> armyType)
    {
        foreach(Unit unit in armyType)
        {
            unit.RemoveTemporaryHP();
        }
    }

    private int FightRound(Speed currentSpeed) //returns 1 for a win, 0 for fight is ongoing, -1 for a loss, 69 for undecided (both armies died)
    {
        if (army.Count <= 0 && enemyArmy.Count <= 0) return 69;
        else if (enemyArmy.Count <= 0) return 1;
        else if (army.Count <= 0) return -1;

        List<List<Unit>> armyBuffs = ApplyBuffs(army, enemyArmy, currentSpeed, true); //attacking army is [0], defending army is [1]
        List<List<Unit>> enemyBuffs = ApplyBuffs(enemyArmy, army, currentSpeed, false);

        List<Unit> armyAfterBuffs = AddArmyValues(army, armyBuffs[0], enemyBuffs[1]);
        List<Unit> enemyAfterBuffs = AddArmyValues(enemyArmy, armyBuffs[1], enemyBuffs[0]);

        List<Unit> enemyTakenDamage = Attack(armyAfterBuffs, enemyAfterBuffs, currentSpeed, true);
        List<Unit> armyTakenDamage = Attack(enemyAfterBuffs, armyAfterBuffs, currentSpeed, false);

        army.Clear();
        army.AddRange(armyTakenDamage);

        enemyArmy.Clear();
        enemyArmy.AddRange(enemyTakenDamage);

        if (army.Where(u => !u.unit.isStructure).ToList().Count <= 0 && enemyArmy.Where(u => !u.unit.isStructure).ToList().Count <= 0) return 69;
        else if (enemyArmy.Where(u => !u.unit.isStructure).ToList().Count <= 0) return 1;
        else if (army.Where(u => !u.unit.isStructure).ToList().Count <= 0) return -1;
        else return 0;
    }

    private List<Unit> AddArmyValues(List<Unit> armyType, List<Unit> values1, List<Unit> values2)
    {
        List<Unit> armyClone = new List<Unit>();
        armyClone.AddRange(armyType);

        for(int i = 0; i < armyClone.Count; i++)
        {
            Unit u1 = values1.Where(u => u.id == armyType[i].id).ToList().Count > 0 ? values1.Where(u => u.id == armyType[i].id).First() : null;
            Unit u2 = values2.Where(u => u.id == armyType[i].id).ToList().Count > 0 ? values2.Where(u => u.id == armyType[i].id).First() : null;

            if (u1 != null && u2 != null)
            {
                armyClone[i].health += (u1.health - armyClone[i].health) + (u2.health - armyClone[i].health);
                armyClone[i].damage += (u1.damage - armyClone[i].damage) + (u2.damage - armyClone[i].damage);
            }
            else Debug.LogError(armyClone[i].unit.name + " not found while trying to add values!");
        }

        return armyClone;
    }

    private List<List<Unit>> ApplyBuffs(List<Unit> attackingArmy, List<Unit> defendingArmy, Speed currentSpeed, bool attackerIsPlayer)
    {
        List<string> attackDone = new List<string>();
        List<Unit> defendingArmyClone = new List<Unit>();
        List<Unit> attackingArmyClone = new List<Unit>();

        defendingArmyClone.AddRange(defendingArmy);
        attackingArmyClone.AddRange(attackingArmy);

        for (int i = 0; i < attackingArmy.Count; i++)
        {
            if (!attackDone.Contains(attackingArmy[i].unit.name) && attackingArmy[i].unit.speed == currentSpeed)
            {
                List<Unit> attackers = new List<Unit> { attackingArmy[i] };
                attackDone.Add(attackingArmy[i].unit.name);

                for (int j = i + 1; j < attackingArmy.Count; j++)
                {
                    if (attackingArmy[i].unit.name == attackingArmy[j].unit.name) attackers.Add(attackingArmy[j]);
                }

                if (attackingArmy[i].unit.specialSkill == Skill.buffAlliedRow)
                {
                    List<Unit> unitsToBuff = new List<Unit>();

                    foreach(Unit attacker in attackers)
                    {
                        foreach(Unit u in attackingArmyClone)
                        {
                            if(u.setType == attacker.setType && u.id != attacker.id)
                            {
                                u.ChangeDamageByValue(attacker.unit.specialSkillValue);
                                Unit unitClone = new Unit(u.unit, u.id, u.health, u.damage);
                                unitsToBuff.Add(unitClone);
                            }
                        }
                    }

                    List<Unit> attackerClones = new List<Unit>();
                    foreach (Unit attacker in attackers) { attackerClones.Add(new Unit(attacker.unit, attacker.id, attacker.ammunition, attacker.skillActive)); }
                    animateUnits.Add(new AnimateUnit(currentSpeed, attackerClones, attackerIsPlayer, unitsToBuff));
                }
                else if(attackingArmy[i].unit.specialSkill == Skill.reduceEnemyDamage)
                {
                    List<Unit> highestDamageUnits = FindHighestDamageUnits(defendingArmyClone);
                    Unit highestDamageUnit = FindUnitByHighestDefensePriority(highestDamageUnits);
                    if (highestDamageUnit == null) break;

                    for (int j = 0; j < attackers.Count; j++)
                    {
                        if (attackers[j].UseAmmunition()) highestDamageUnit.ChangeDamageByValue(-attackers[j].unit.specialSkillValue);
                        else
                        {
                            attackers.Remove(attackers[j]);
                            j--;
                        }
                    }

                    foreach (Unit attacker in attackers)
                    {
                        if (attacker.skillActive && attacker.ammunition == 0)
                        {
                            attacker.EnableSkill(false);
                        }
                    }

                    if (attackers.Count >= 1)
                    {
                        List<Unit> attackerClones = new List<Unit>();
                        foreach (Unit attacker in attackers) { attackerClones.Add(new Unit(attacker.unit, attacker.id, attacker.ammunition, attacker.skillActive)); }
                        Unit defenderClone = new Unit(highestDamageUnit.unit, highestDamageUnit.id, highestDamageUnit.health, highestDamageUnit.damage);
                        animateUnits.Add(new AnimateUnit(currentSpeed, attackerClones, defenderClone, attackerIsPlayer));
                    }
                }
                else if(attackingArmy[i].unit.specialSkill == Skill.healWeakestUnitInRow)
                {
                    List<Unit> unitsToHeal = new List<Unit>();

                    foreach (Unit attacker in attackers)
                    {
                        List<Unit> unitsInRow = new List<Unit>();

                        foreach (Unit u in attackingArmyClone)
                        {
                            if (u.setType == attacker.setType && u.id != attacker.id) unitsInRow.Add(u);
                        }

                        if (unitsInRow.Count < 1) continue;

                        List<Unit> weakestUnitsInRow = FindWeakestUnits(unitsInRow);
                        Unit weakestUnitInRow = FindUnitByHighestDefensePriority(weakestUnitsInRow);

                        weakestUnitInRow.ChangeHealthByValue(attacker.unit.specialSkillValue);

                        Unit unitClone = new Unit(weakestUnitInRow.unit, weakestUnitInRow.id, weakestUnitInRow.health, weakestUnitInRow.damage);
                        unitsToHeal.Add(unitClone);
                    }

                    if(unitsToHeal.Count >= 1)
                    {
                        List<Unit> attackerClones = new List<Unit>();
                        foreach (Unit attacker in attackers) { attackerClones.Add(new Unit(attacker.unit, attacker.id, attacker.ammunition, attacker.skillActive)); }
                        animateUnits.Add(new AnimateUnit(currentSpeed, attackerClones, attackerIsPlayer, unitsToHeal));
                    }
                }
            }
        }

        List<List<Unit>> returnValue = new List<List<Unit>>
        {
            attackingArmyClone,
            defendingArmyClone
        };
        return returnValue;
    }

    private List<Unit> Attack(List<Unit> attackingArmy, List<Unit> defendingArmy, Speed currentSpeed, bool attackerIsPlayer) //Returns an updated List for the damaged Army
    {
        List<string> attackDone = new List<string>();
        List<Unit> defendingArmyClone = new List<Unit>();

        defendingArmyClone.AddRange(defendingArmy);

        for (int i = 0; i < attackingArmy.Count; i++)
        {
            if (!attackDone.Contains(attackingArmy[i].unit.name) && attackingArmy[i].unit.speed == currentSpeed)
            {
                List<Unit> attackers = new List<Unit> { attackingArmy[i] };
                attackDone.Add(attackingArmy[i].unit.name);

                for (int j = i + 1; j < attackingArmy.Count; j++)
                {
                    if (attackingArmy[i].unit.name == attackingArmy[j].unit.name && attackingArmy[i].skillActive == attackingArmy[j].skillActive)
                    {
                        attackers.Add(attackingArmy[j]);
                    }
                }

                if (attackingArmy[i].unit.specialSkill == Skill.attackWeakestUnits)
                {
                    int damage = 0;
                    List<Unit> defendersAttackedClone = new List<Unit>();
                    foreach (Unit attacker in attackers) { damage += attacker.damage; }

                    for (int j = 0; damage > 0 && j < 1000; j++)
                    {
                        List<Unit> weakestUnits = FindWeakestUnits(defendingArmyClone);
                        Unit weakestUnit = FindUnitByHighestDefensePriority(weakestUnits != null ? weakestUnits : defendingArmyClone);
                        if (weakestUnit == null) break;

                        damage = weakestUnit.Hit(damage);

                        defendersAttackedClone.Add(new Unit(weakestUnit.unit, weakestUnit.id, weakestUnit.health, weakestUnit.damage));

                        if (weakestUnit.health <= 0) defendingArmyClone.Remove(weakestUnit);
                        if (defendingArmyClone.Count <= 0) break;
                    }

                    List<Unit> attackerClones = new List<Unit>();
                    foreach (Unit attacker in attackers) { attackerClones.Add(new Unit(attacker.unit, attacker.id, attacker.ammunition, attacker.skillActive)); }
                    animateUnits.Add(new AnimateUnit(currentSpeed, attackerClones, defendersAttackedClone, attackerIsPlayer));
                }
                else if(attackingArmy[i].unit.specialSkill == Skill.snipeWeakestUnit && attackingArmy[i].skillActive)
                { 
                    int damage = 0;
                    foreach(Unit attacker in attackers)
                    {
                        if(attacker.UseAmmunition()) damage += attacker.damage; 
                        else Debug.LogError("Sniper attacks with special skill when no munition is left - should attack normally!");
                    }

                    List<Unit> highestDamageUnits = FindHighestDamageUnits(defendingArmyClone);
                    Unit highestDamageUnit = FindUnitByHighestDefensePriority(highestDamageUnits != null ? highestDamageUnits : defendingArmyClone);
                    if (highestDamageUnit == null) break;

                    highestDamageUnit.Hit(damage);

                    foreach(Unit attacker in attackers) 
                    {
                        if(attacker.skillActive && attacker.ammunition == 0)
                        {
                            attacker.EnableSkill(false);
                            attacker.ChangeDamageByValue(-attacker.unit.specialSkillValue);
                        }
                    }

                    List<Unit> attackerClones = new List<Unit>();
                    foreach (Unit attacker in attackers) { attackerClones.Add(new Unit(attacker.unit, attacker.id, attacker.ammunition, attacker.skillActive)); }
                    Unit unitClone = new Unit(highestDamageUnit.unit, highestDamageUnit.id, highestDamageUnit.health, highestDamageUnit.damage);
                    animateUnits.Add(new AnimateUnit(currentSpeed, attackerClones, unitClone, attackerIsPlayer));

                    if (highestDamageUnit.health <= 0) defendingArmyClone.Remove(highestDamageUnit);
                    if (defendingArmyClone.Count <= 0) break;
                }
                else
                {
                    int damage = 0;
                    List<Unit> defendersAttackedClone = new List<Unit>();
                    foreach (Unit attacker in attackers) { damage += attacker.damage; }

                    for (int j = 0; damage > 0 && j < 1000; j++)
                    {
                        Unit nextUnit = FindUnitByHighestDefensePriority(defendingArmyClone);
                        if (nextUnit == null) break;

                        damage = nextUnit.Hit(damage);

                        defendersAttackedClone.Add(new Unit(nextUnit.unit, nextUnit.id, nextUnit.health, nextUnit.damage));

                        if (nextUnit.health <= 0) defendingArmyClone.Remove(nextUnit);
                        if (defendingArmyClone.Count <= 0) break;
                    }

                    List<Unit> attackerClones = new List<Unit>();
                    foreach (Unit attacker in attackers) { attackerClones.Add(new Unit(attacker.unit, attacker.id, attacker.ammunition, attacker.skillActive)); }
                    animateUnits.Add(new AnimateUnit(currentSpeed, attackerClones, defendersAttackedClone, attackerIsPlayer));
                }
            }
        }

        return defendingArmyClone;
    }

    private List<Unit> FindHighestDamageUnits(List<Unit> pool)
    {
        if (pool == null || pool.Count < 1) return null;

        pool = pool.Where(u => u.setType != Type.leader).ToList();
        if (pool == null || pool.Count < 1) return null;

        List<Unit> highestDamageUnit = new List<Unit>();
        highestDamageUnit.Add(pool[0]);

        for (int i = 1; i < pool.Count; i++)
        {
            if (pool[i].damage > highestDamageUnit[0].damage)
            {
                highestDamageUnit.Clear();
                highestDamageUnit.Add(pool[i]);
            }
            else if (pool[i].damage == highestDamageUnit[0].damage)
            {
                highestDamageUnit.Add(pool[i]);
            }
        }

        return highestDamageUnit;
    }

    private List<Unit> FindWeakestUnits(List<Unit> pool)
    {
        if (pool == null || pool.Count < 1) return null;

        pool = pool.Where(u => u.setType != Type.leader).ToList();
        if (pool == null || pool.Count < 1) return null;

        List<Unit> lowestHealthUnits = new List<Unit>();
        lowestHealthUnits.Add(pool[0]);

        for(int i = 1; i < pool.Count; i++)
        {
            if (pool[i].health < lowestHealthUnits[0].health)
            {
                lowestHealthUnits.Clear();
                lowestHealthUnits.Add(pool[i]);
            }
            else if(pool[i].health == lowestHealthUnits[0].health)
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

    private void ResetGame(bool completeReset)
    {
        //Reset Variables
        round = 0;
        roundsUntilFight = roundsUntilBeginningFight;
        roundOver = false;
        yourTurn = false;
        
        //Reset UI
        armyUI.SetCompleteArmyUI(null);
        enemyUI.SetCompleteArmyUI(null);

        //Save structures
        foreach(Unit u in army) { if (u.unit.isStructure) armyStructures.Add(u); }
        foreach(Unit u in enemyArmy) { if (u.unit.isStructure) enemyStructures.Add(u); }

        //Clear arrays
        army.Clear();
        enemyArmy.Clear();
        animateUnits.Clear();

        //Enable/Disable gameObjects
        btn_nextRound.enabled = false;

        //Stop listening for card dragging
        gameUI.ListenForCardMovement(false);

        if (completeReset)
        {
            //Reset Score
            score = Vector2Int.zero;
            armyUI.SetLeaderHealth(2);
            enemyUI.SetLeaderHealth(2);

            //Reset player hand
            playerHandUI.SetHandUI(null);

            //Clear Hand and Deck arrays
            playerHand.Clear();
            enemyHand.Clear();
            playerDeck.Clear();
            enemyDeck.Clear();

            //Add cards to deck again
            AddTestingCardsToDeck();

            //Select Leader
            selectLeaderUI.gameObject.SetActive(true);
            selectLeaderUI.DisplayLeaders(playerDeck);
            gameUI.ListenForCardSelection(true);
        }
        else
        {
            //Reset Army Leader
            Unit armyLeaderReset = new Unit(armyLeader.unit, armyLeader.id);
            Unit enemyLeaderReset = new Unit(armyLeader.unit, armyLeader.id);

            army.Add(armyLeaderReset);
            enemyArmy.Add(enemyLeaderReset);

            armyLeader = armyLeaderReset;
            enemyLeader = enemyLeaderReset;

            //Add existing structures
            army.AddRange(armyStructures);
            enemyArmy.AddRange(enemyStructures);

            armyUI.SetCompleteArmyUI(army);
            enemyUI.SetCompleteArmyUI(enemyArmy);
        }
    }

    private void AddTestingCardsToDeck()
    {
        int id = 1;

        FileController files = FileController.instance;

        for (int i = 0; i < 7; i++) { playerDeck.Add(new Unit(files.simpleFighter, id)); id++; }
        for (int i = 0; i < 4; i++) { playerDeck.Add(new Unit(files.shieldsquire, id)); id++; }
        for (int i = 0; i < 3; i++) { playerDeck.Add(new Unit(files.horseman, id)); id++; }
        for (int i = 0; i < 4; i++) { playerDeck.Add(new Unit(files.conquistador, id)); id++; }
        for (int i = 0; i < 6; i++) { playerDeck.Add(new Unit(files.simpleArcher, id)); id++; }
        for (int i = 0; i < 4; i++) { playerDeck.Add(new Unit(files.catapult, id)); id++; }
        for (int i = 0; i < 2; i++) { playerDeck.Add(new Unit(files.sharpshooter, id)); id++; }
        for (int i = 0; i < 2; i++) { playerDeck.Add(new Unit(files.sleepDartNinja, id)); id++; }
        for (int i = 0; i < 2; i++) { playerDeck.Add(new Unit(files.battleDrummer, id)); id++; }
        for (int i = 0; i < 2; i++) { playerDeck.Add(new Unit(files.fieldMedic, id)); id++; }
        playerDeck.Add(new Unit(files.kingJohn, id)); id++;

        for (int i = 0; i < 7; i++) { enemyDeck.Add(new Unit(files.simpleFighter, id)); id++; }
        for (int i = 0; i < 4; i++) { enemyDeck.Add(new Unit(files.shieldsquire, id)); id++; }
        for (int i = 0; i < 3; i++) { enemyDeck.Add(new Unit(files.horseman, id)); id++; }
        for (int i = 0; i < 4; i++) { enemyDeck.Add(new Unit(files.conquistador, id)); id++; }
        for (int i = 0; i < 6; i++) { enemyDeck.Add(new Unit(files.simpleArcher, id)); id++; }
        for (int i = 0; i < 4; i++) { enemyDeck.Add(new Unit(files.catapult, id)); id++; }
        for (int i = 0; i < 2; i++) { enemyDeck.Add(new Unit(files.sharpshooter, id)); id++; }
        for (int i = 0; i < 2; i++) { enemyDeck.Add(new Unit(files.sleepDartNinja, id)); id++; }
        for (int i = 0; i < 2; i++) { enemyDeck.Add(new Unit(files.battleDrummer, id)); id++; }
        for (int i = 0; i < 2; i++) { enemyDeck.Add(new Unit(files.fieldMedic, id)); id++; }
        for (int i = 0; i < 1; i++) { enemyDeck.Add(new Unit(files.castleWalls, id)); id++; }
        enemyDeck.Add(new Unit(files.kingJohn, id)); id++;
    }
}
