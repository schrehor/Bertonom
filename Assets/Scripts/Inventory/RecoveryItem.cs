using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] private int hpAmount;
    [SerializeField] private bool restoreMaxHP;

    [Header("PP")] 
    [SerializeField] private int ppAmount;
    [SerializeField] private bool restoreMaxPP;

    [Header("Status Condition")] 
    [SerializeField] private ConditionID status;
    [SerializeField] private bool recoverAllStatus;

    [Header("Revivde")] 
    [SerializeField] private bool revive;
    [SerializeField] private bool maxRevive;


}
