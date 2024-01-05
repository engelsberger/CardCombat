using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MouseOverTextUI : MonoBehaviour
{
    private RectTransform textField;
    public TMP_Text txt_text;

    private static Vector2 originalSize = new Vector2(200f, 25f);
    private static float increaseYPerAdditionalRow = 16f;


    public void SetMouseOverText(string text)
    {
        txt_text.text = text;
        txt_text.ForceMeshUpdate();

        Vector2 textFieldSize = originalSize;
        for(int i = 1; i < txt_text.textInfo.lineCount; i++) { textFieldSize.y += increaseYPerAdditionalRow; }

        textField = GetComponent<RectTransform>();
        textField.sizeDelta = textFieldSize;
    }
}
