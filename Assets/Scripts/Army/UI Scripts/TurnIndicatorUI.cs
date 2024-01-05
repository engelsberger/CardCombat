using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnIndicatorUI : MonoBehaviour
{
    public RectTransform turnIndicator;
    public Image img_speed;

    private Vector3 playerTurnScale = new Vector3(1, 1, 1);
    private Vector3 enemyTurnScale = new Vector3(-1, 1, 1);



    public void SetTurnIndicator(bool playerTurn, Speed speed)
    {
        if (!turnIndicator.gameObject.activeInHierarchy) turnIndicator.gameObject.SetActive(true);
        if (!img_speed.enabled) img_speed.enabled = true;

        if (playerTurn) turnIndicator.localScale = playerTurnScale;
        else turnIndicator.localScale = enemyTurnScale;

        switch (speed)
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
                Debug.LogError("Speed not taken care of!");
                break;
        }
    }

    public void SetTurnIndicator(bool activate)
    {
        turnIndicator.gameObject.SetActive(activate);
        img_speed.enabled = activate;
    }
}
