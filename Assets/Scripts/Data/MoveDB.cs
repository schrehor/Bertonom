using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDB
{
    static Dictionary<string, MoveBase> moves;

    public static void Init()
    {
        moves = new Dictionary<string, MoveBase>();

        var movesArray = Resources.LoadAll<MoveBase>("");

        foreach (var move in movesArray)
        {
            if (moves.ContainsKey(move.Name))
            {
                Debug.Log($"There are 2 pokemon with the name {move.Name}");
                continue;
            }

            moves[move.Name] = move;
        }
    }

    public static MoveBase GetPokemonByName(string name)
    {
        if (!moves.ContainsKey(name))
        {
            Debug.LogError($"Pokemon with name {name} not foung in the database");
            return null;
        }

        return moves[name];
    }
}
