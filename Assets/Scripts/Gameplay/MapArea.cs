using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildPokemons;

    /// <summary>
    /// Picks a random pokemon from the list of wildPokemons that are set in the area.
    /// </summary>
    /// <returns>A random Pokemon</returns>
    public Pokemon GetRandomWildPokemon()
    {
        var wildPokemon = wildPokemons[Random.Range(0, wildPokemons.Count)];
        wildPokemon.Init();
        return wildPokemon;
    }
}
