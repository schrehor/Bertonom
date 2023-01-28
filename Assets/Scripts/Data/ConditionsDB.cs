using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } =
        new Dictionary<ConditionID, Condition>()
        {
            {
                ConditionID.psn, 
                new Condition()
                {
                    Name = "Poison",
                    StartMessage = "has been poisoned",
                    OnAfterTurn = pokemon =>
                    {
                        pokemon.UpdateHP(pokemon.MaxHp / 8);
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.PkmName} hurt itself due to poison");
                    }
                    
                }
            },
            {
                ConditionID.brn, 
                new Condition()
                {
                    Name = "Burn",
                    StartMessage = "has been burned",
                    OnAfterTurn = pokemon =>
                    {
                        pokemon.UpdateHP(pokemon.MaxHp / 16);
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.PkmName} hurt itself due to burn");
                    }
                    
                }
            }
        };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz
}