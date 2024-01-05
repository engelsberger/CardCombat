using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitController : MonoBehaviour
{
    public Unit unit;
    public Image img_unit;
    public Image img_specialSkill;
    private Image img_background;
    public Image img_ammunition;
    public Image img_leaderIcon;
    public Image img_speed;
    public TMP_Text txt_name;
    public TMP_Text txt_damage;
    public TMP_Text txt_health;
    public TMP_Text txt_ammunition;


    private void Awake()
    {
        img_background = GetComponent<Image>();
    }


    public void SetUnit(Unit unit)
    {
        if (unit == null)
        {
            this.unit = null;
            img_unit.sprite = null;
            img_specialSkill = null;
            txt_name.text = "NAME";
            txt_damage.text = "DMG";
            txt_health.text = "HEALTH";
            txt_ammunition.text = "AMMO";
            return;
        }

        this.unit = unit;
        img_unit.sprite = unit.unit.sprite;
        txt_name.text = unit.unit.name;
        SetHealth(unit.health);
        SetDamage(unit.damage);
        SetAmmunition(unit.ammunition);
        ActivateSpecialSkill(unit.skillActive);
        if (unit.setType == Type.leader) img_leaderIcon.enabled = true;
        else img_leaderIcon.enabled = false;

        switch (unit.unit.speed)
        {
            case Speed.fast:
                img_speed.sprite = FileController.instance.speed_fast;
                break;
            case Speed.normal:
                img_speed.sprite = FileController.instance.speed_normal;
                break;
            case Speed.slow:
                img_speed.sprite = FileController.instance.speed_slow;
                break;
            default:
                img_speed.enabled = false;
                Debug.LogError("Unit has no speed assigned!");
                break;
        }

        switch (unit.unit.specialSkill)
        {
            case Skill.attackWeakestUnits:
                img_specialSkill.sprite = FileController.instance.skill_attackWeakestUnit;
                break;
            case Skill.snipeWeakestUnit:
                img_specialSkill.sprite = FileController.instance.skill_attackWeakestUnit;
                break;
            case Skill.reduceEnemyDamage:
                img_specialSkill.sprite = FileController.instance.skill_reduceEnemyDamage;
                break;
            case Skill.buffAlliedRow:
                img_specialSkill.sprite = FileController.instance.skill_buffAlliedRow;
                break;
            case Skill.healWeakestUnitInRow:
                img_specialSkill.sprite = FileController.instance.skill_healAllies;
                break;
            case Skill.increasedDamageToStructures:
                img_specialSkill.sprite = FileController.instance.skill_increasedDamageToStructures;
                break;
            default:
                img_specialSkill.enabled = false;
                break;
        }
    }

    public void SetHealth(int value)
    {
        txt_health.text = value.ToString();
    }

    public void SetDamage(int value)
    {
        txt_damage.text = value.ToString();
    }

    public void SetColor(Color color)
    {
        if (color == null) img_background.color = Color.white;

        img_background.color = color;
    }

    public void SetAmmunition(int value)
    {
        if (value == -1)
        {
            txt_ammunition.gameObject.SetActive(false);
            img_ammunition.gameObject.SetActive(false);
        }
        else
        {
            txt_ammunition.gameObject.SetActive(true);
            img_ammunition.gameObject.SetActive(true);
            txt_ammunition.text = value.ToString();
        }
    }

    public void ActivateSpecialSkill(bool activate)
    {
        if (activate) img_specialSkill.color = new Color(img_specialSkill.color.r, img_specialSkill.color.g, img_specialSkill.color.b, 1f);
        else img_specialSkill.color = new Color(img_specialSkill.color.r, img_specialSkill.color.g, img_specialSkill.color.b, 0.3f);
    }
}
