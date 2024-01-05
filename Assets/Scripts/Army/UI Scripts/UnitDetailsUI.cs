using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UnitDetailsUI : MonoBehaviour
{
    private GameController gameController;
    private Unit unit;
    public Image img_unit;
    public TMP_Text txt_name;
    public TMP_Text txt_stats;
    public GameObject descriptionSpecialValue;
    public GameObject specialSkillText;
    public RectTransform descriptionText;
    public GameObject questionmarkSpecialValueObject;
    public Button btn_select;
    public MouseOverUIData questionmarkHealth;
    public MouseOverUIData questionmarkDamage;
    public MouseOverUIData questionmarkSpeed;
    public MouseOverUIData questionmarkPosition;
    public MouseOverUIData questionmarkSpecialValue;

    private static float descriptionTextHighYPosition = -80f;
    private static float descriptionTextLowYPosition = -115f;


    public void SetUnitDetails(Unit unit, bool selectEnabled = false)
    {
        gameController = GameController.instance;
        this.unit = unit;

        btn_select.onClick.AddListener(BtnSelect);

        btn_select.gameObject.SetActive(selectEnabled);
        img_unit.sprite = unit.unit.sprite;

        string stats = unit.health + " (" + unit.unit.maxHealth + ")";
        string description = unit.unit.description;

        stats += "\n" + unit.damage + " (" + unit.unit.maxDamage + ")";
        stats += "\n" + unit.unit.speed.ToString();

        if (unit.unit.type == Type.firstRow) stats += "\n1. Row";
        else if (unit.unit.type == Type.secondRow) stats += "\n2. Row";
        else stats += "\n" + unit.unit.type.ToString();

        if(unit.unit.specialSkill != Skill.none)
        {
            TMP_Text txt_descriptionSpecialValue = descriptionSpecialValue.GetComponent<TMP_Text>();

            specialSkillText.SetActive(true);
            descriptionText.anchoredPosition = new Vector2(0, descriptionTextLowYPosition);

            string specialSkill = string.Empty;
            switch (unit.unit.specialSkill)
            {
                case Skill.attackWeakestUnits:
                    specialSkill = "Attacks the weakest enemy unit first";
                    description += " Will attack the enemy unit with the lowest current HP.";
                    break;
                case Skill.snipeWeakestUnit:
                    specialSkill = "Attacks the highest damage unit first";
                    description += " Attacks the enemy unit with the highest current damage. Will only attack one unit per turn if this skill is active." +
                        " When he runs out of ammunition, he will swap to his bayonet, attacking normally.";
                    questionmarkSpecialValue.SetData("While he uses his crossbow, his damage will be boosted by this value.");
                    txt_descriptionSpecialValue.text = "Snipe damage bonus";
                    break;
                case Skill.reduceEnemyDamage:
                    specialSkill = "Reduces the damage of an enemy unit";
                    description += " Will reduce the damamge of the enemy unit that currently has the highest damage number by " + unit.unit.specialSkillValue + ". "
                        + "This damage reduction happens every time this unit attacks and can stack. A units damage cannot go below one.";
                    questionmarkSpecialValue.SetData("Reduces the damage of the enemy unit with the highest current damage by this value. A units damage cannot go below one.");
                    txt_descriptionSpecialValue.text = "Damage reduction";
                    break;
                case Skill.buffAlliedRow:
                    specialSkill = "Buffs damage of all units in the same row.";
                    description += " Will increase the damage of all units in the same row he is placed by " + unit.unit.specialSkillValue
                        + ". This damage increase happens every time this unit plays a turn and it can stack.";
                    questionmarkSpecialValue.SetData("Will increase the damage of all units in the same row by this value.");
                    txt_descriptionSpecialValue.text = "Damage increase";
                    break;
                case Skill.healWeakestUnitInRow:
                    specialSkill = "Heals the weakest unit in the same row.";
                    description += " Will heal the weakest unit in the same row by " + unit.unit.specialSkillValue + " every time he gets to play a turn. Any health above maximum" +
                        " will be removed at the end of a round.";
                    questionmarkSpecialValue.SetData("Will heal the unit with the lowest current hp in the same row by this value.");
                    txt_descriptionSpecialValue.text = "Health increase";
                    break;
                default:
                    Debug.LogError("Special Skill not described");
                    break;
            }
            specialSkillText.GetComponent<TMP_Text>().text = specialSkill;
        }
        else
        {
            specialSkillText.SetActive(false);
            descriptionText.anchoredPosition = new Vector2(0, descriptionTextHighYPosition);
        }

        if (unit.unit.specialSkillValue != 0)
        {
            descriptionSpecialValue.SetActive(true);
            questionmarkSpecialValueObject.SetActive(true);
            stats += "\n" + unit.unit.specialSkillValue;
        }
        else
        {
            descriptionSpecialValue.SetActive(false);
            questionmarkSpecialValueObject.SetActive(false);
        }

        txt_name.text = unit.unit.name;
        txt_stats.text = stats;
        descriptionText.GetComponent<TMP_Text>().text = description;

        questionmarkHealth.SetData("Displays the units current health, within the brackets it shows the units max health. Any health gained that is above max health will be " +
            "removed at the end of the round.");
        questionmarkDamage.SetData("Displays the units current damage, within the brackets it shows the units starting damage.");
        questionmarkSpeed.SetData("This is the units speed. There are three variants of speed: fast, normal and slow." +
            " Fast units will attack first, after that come the normal units and siege units make the last attack.");
        questionmarkPosition.SetData("Displays the position on the battlefield the unit will be placed in. This determines which unit will get attacked first." +
            " Units of a row will only be attacked if every unit in the row in front of them is dead, except the attacker has a special ability that allows him to ignore this rule.");
    }

    private void BtnSelect()
    {
        gameController.LeaderSelected(unit);
        Destroy(gameObject);
    }
}
