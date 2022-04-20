using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    PokemonBase pkmBase;
    int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        pkmBase = pBase;
        level = pLevel; 
    }

    public int Attack
    {
        get { return Mathf.FloorToInt((pkmBase.Attack * level) / 100f) + 5; }
    }

    public int Defense
    {
        get { return Mathf.FloorToInt((pkmBase.Defense * level) / 100f) + 5; }
    }

    public int SpAttack
    {
        get { return Mathf.FloorToInt((pkmBase.SpAttack * level) / 100f) + 5; }
    }

    public int SpDefense
    {
        get { return Mathf.FloorToInt((pkmBase.SpDefense * level) / 100f) + 5; }
    }

    public int Speed
    {
        get { return Mathf.FloorToInt((pkmBase.Speed * level) / 100f) + 5; }
    }

    public int MaxHp
    {
        get { return Mathf.FloorToInt((pkmBase.MaxHp * level) / 100f) + 10; }
    }
}
