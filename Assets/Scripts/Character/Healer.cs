using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;
        
        yield return DialogManager.Instance.ShowDialog(dialog, new List<string> { "Yes", "No" }, (choiceIndex) =>
        {
            selectedChoice = choiceIndex;
        });

        // Yes
        if (selectedChoice == 0)
        {
            yield return Fader.Instance.FadeIn(0.5f);
            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(x => x.Heal());
            playerParty.PartyUpdated();
            yield return Fader.Instance.FadeOut(0.5f);
            
            yield return DialogManager.Instance.ShowDialogText($"Daj si stareho tobiho, je dorby na nervy. A daj aj svojim pokesom.");
        }
        // No
        else if (selectedChoice == 1)
        {
            yield return DialogManager.Instance.ShowDialogText($"Tak pakuj dopice do prdele");
        }
    }
}
