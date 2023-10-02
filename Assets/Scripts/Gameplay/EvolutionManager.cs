using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] private GameObject evolutionUI;
    [SerializeField] private Image pokemonImage;

    [SerializeField] private AudioClip evolutionMusic;


    public event Action OnStartEvolution;
    public event Action OnEndEvolution;
    public static EvolutionManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);
        
        AudioManager.Instance.PlayMusic(evolutionMusic);
        
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is evolving!");
        var oldPokemon = pokemon.Base;
        
        pokemon.Evolve(evolution);
        
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldPokemon.Name} evolved into {pokemon.Base.Name}!");
        
        evolutionUI.SetActive(false);
        OnEndEvolution?.Invoke();
    }

}
