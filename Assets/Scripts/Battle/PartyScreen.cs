using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;
    int selection;
    private PokemonParty _party;

    public Pokemon SelectedMember => pokemons[selection];

    /// <summary>
    /// Party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    /// </summary>
    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        _party = PokemonParty.GetPlayerParty();
        SetPartyData();

        _party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        pokemons = _party.Pokemons;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemeberSelection(selection);

        messageText.text = "Choose a Pokemon";
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        int prevSelection = selection;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selection += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selection -= 2;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selection--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selection++;
        }

        selection = Mathf.Clamp(selection, 0, pokemons.Count - 1);

        if (selection != prevSelection)
        {
            UpdateMemeberSelection(selection);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            onBack?.Invoke();
        }
    }

    public void UpdateMemeberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }

    public void ShowIfTmUsable(TmItem tmItem)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            string message = tmItem.CanBeTaught(pokemons[i]) ? "ABLE" : "UNABLE";
            memberSlots[i].SetMessage(message);
        }
    }
    
    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }
}
