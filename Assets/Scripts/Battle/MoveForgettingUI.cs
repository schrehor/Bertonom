using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveForgettingUI : MonoBehaviour
{
    [SerializeField] List<Text> moveText;
    [SerializeField] Color highLightColor;

    int currentSelection = 0;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveText[i].text = currentMoves[i].Name;
        }

        moveText[currentMoves.Count].text = newMove.Name;
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection--;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumOfMoves);

        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.X))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < PokemonBase.MaxNumOfMoves + 1; i++)
        {
            if (i == selection)
            {
                moveText[i].color = highLightColor;
            }
            else
            {
                moveText[i].color = Color.black;
            }
        }
    }
}
