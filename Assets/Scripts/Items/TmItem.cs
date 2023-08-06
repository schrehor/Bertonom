using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] private MoveBase move;
    [SerializeField] bool isHM;

    public MoveBase Move => move;
    public bool IsHM => isHM;

    public override bool Use(Pokemon pokemon)
    {
        // Learning move is handled form InventoryUI
        // If the move was learned return true
        return pokemon.HasMove(move);
    }

    public bool CanBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.LearnableByItems.Contains(move);     
    }
    
    public override bool IsReusable => isHM;
    public override bool CanUseInBattle => false;
    public override string Name => base.Name + $": {move.Name}";
}
