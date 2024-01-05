using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOverUIData : MonoBehaviour
{
    private string data;

    
    public void SetData(string data) { this.data = data; }
    public string GetData() { return data; }
}
