using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new pokeball")]
public class PokeballItem : ItemBase
{
    [SerializeField] private float catchRateModifier = 1f;

    public float CatchRateModifier => catchRateModifier;

    public override bool Use(Pokemon pokmeon)
    {
        if (GameController.Instance.State == GameState.Battle)
        {
            return true;
        }
        return false;
    }
}
