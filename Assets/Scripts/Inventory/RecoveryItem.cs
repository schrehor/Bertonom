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

    public override bool Use(Pokemon pokemon)
    {
        // Revive
        if (revive || maxRevive)
        {
            if (pokemon.HP > 0)
            {
                return false;
            }

            if (revive)
            {
                pokemon.IncreaseHP(pokemon.MaxHp / 2);
            }
            else
            {
                pokemon.IncreaseHP(pokemon.MaxHp);
            }
            
            pokemon.CureStatus();
            return true;
        }

        // No other items can be used on fainted pokemon
        if (pokemon.HP == 0)
        {
            return false;
        }
        
        // Restore HP
        if (hpAmount > 0 || restoreMaxHP)
        {
            if (pokemon.HP == pokemon.MaxHp)
            {
                return false;
            }

            if (restoreMaxHP)
            {
                pokemon.IncreaseHP(pokemon.MaxHp);
            }
            else
            {
                pokemon.IncreaseHP(hpAmount);  
            }
        }

        return true;
    }
}
