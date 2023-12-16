using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonGiver : MonoBehaviour
{
    [SerializeField] private Pokemon pokemonToGive;
    [SerializeField] private Dialog dialog;

    private bool _used = false;
    
    public IEnumerator GivePokemon(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);
        
        pokemonToGive.Init();
        player.GetComponent<PokemonParty>().AddPokemon(pokemonToGive);
        _used = true;

		AudioManager.Instance.PlaySfx(AudioId.PokemonObtained, pauseMusic: true);

		string dialogText = $"{player.Name} received {pokemonToGive.Base.Name}";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return pokemonToGive != null && !_used;
    }

    public object CaptureState()
    {
        return _used;
    }

    public void RestoreState(object state)
    {
        _used = (bool) state;
    }
}
