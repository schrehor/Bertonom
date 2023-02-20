using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlightedColor;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionText;
    [SerializeField] List<Text> moveText;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    [SerializeField] Text yesText;
    [SerializeField] Text noText;

    /// <summary>
    /// Instantly writes given <c>dialog</c>.
    /// </summary>
    /// <param name="dialog">Dialog</param>
    public void SetDialog(string dialog)
    { 
        dialogText.text = dialog; 
    }

    /// <summary>
    /// Types the given <c>dialog</c> letter by letter.
    /// </summary>
    /// <param name="dialog">Dialog</param>
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText (bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }
    
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    /// <summary>
    /// Highlights text in Action selection
    /// </summary>
    /// <param name="selectedAction">Number representing the selection in order they were made</param>
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionText.Count; i++)
        {
            if (i == selectedAction)
            {
                actionText[i].color = highlightedColor;
            }
            else
            {
                actionText[i].color = Color.black;
            }
        }
    }

    /// <summary>
    /// Highlights text in Move selection
    /// </summary>
    /// <param name="selectedMove">Number representing the selected move</param>
    /// <param name="move">Move that is selected</param>
    public void UpdatMoveSelection(int selectedMove, Move move)
    {
        for (int i = 0; i < moveText.Count; i++)
        {
            if (i == selectedMove)
            {
                moveText[i].color = highlightedColor;
            }
            else
            {
                moveText[i].color= Color.black;
            }
        }

        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if (move.PP == 0)
        {
            ppText.color = Color.red;
        }
        else if (move.PP <= move.Base.PP / 2)
        {
            ppText.color = new Color(1f, 0.647f, 0f);
        }
        else
        {
            ppText.color = Color.black;
        }
    }

    /// <summary>
    /// Displays the names of moves the Pokemon has.
    /// </summary>
    /// <param name="moves">List of moves the Pokemon has</param>
    public void SetMovesNames(List<Move> moves)
    {
        for (int i = 0; i < moveText.Count; i++)
        {
            if ( i < moves.Count)
            {
                moveText[i].text = moves[i].Base.Name;
            }
            else
            {
                moveText[i].text = "-";
            }
        }
    }

    /// <summary>
    /// Highlights text in the Choice box.
    /// </summary>
    /// <param name="yesSelector">A boolean representing either yes or no choice</param>
    public void UpdateChoiceBox(bool yesSelector)
    {
        if (yesSelector)
        {
            yesText.color = highlightedColor;
            noText.color = Color.black;
        }
        else
        {
            noText.color = highlightedColor;
            yesText.color = Color.black;
        }
    }
}
