using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceText : MonoBehaviour
{ 
    public Text TextField { get; private set; }
    private void Awake()
    {
        TextField = GetComponent<Text>();
    }
}
