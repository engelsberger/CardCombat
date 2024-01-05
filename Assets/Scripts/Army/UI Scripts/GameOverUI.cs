using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TMP_Text txt_gameResults;
    public Button btn_playAgain;


    private void Awake()
    {
        btn_playAgain.onClick.AddListener(BtnPlayAgain);
    }


    public void SetWinScreen(bool playerWins)
    {
        if (playerWins) txt_gameResults.text = "You Win!";
        else txt_gameResults.text = "You Lose!";
    }

    private void BtnPlayAgain()
    {
        gameObject.SetActive(false);

        GameController.instance.FullReset();
    }
}
