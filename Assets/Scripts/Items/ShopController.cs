using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator StartTrading(Merchant merchant)
    {
        int selectedChoice = 0;
        
        yield return DialogManager.Instance.ShowDialogText($"Nazdaar, nechces nejake drogy?", 
            waitForInput: false,
            choices: new List<string>() { "Urcite ano", "Nie ale predam ti", "Fuj ty skurveny droger vyjebany pojebany" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Buy
        }
        else if (selectedChoice == 1)
        {
            //Sell
        }
        else if (selectedChoice == 2)
        {
            yield break;
        }
    }
}
