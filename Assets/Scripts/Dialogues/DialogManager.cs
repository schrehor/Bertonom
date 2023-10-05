using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private Text dialogText;
    [SerializeField] private int lettersPerSecond;
    [SerializeField] private ChoiceBox choiceBox;

    public event Action OnShowDialog;
    public event Action OnDialogFinished;
    
    public bool IsShowing { get; private set; } 

    public static DialogManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowDialogText(string text, bool waitForInput = true, bool autoClose = true, 
        List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);
        
        AudioManager.Instance.PlaySfx(AudioId.UISelect);
        yield return TypeDialog(text);

        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.X));
        }
        
        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        if (autoClose)
        {
            CloseDialog();
        }
        OnDialogFinished?.Invoke();
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        IsShowing = false;
    }
    
    public IEnumerator ShowDialog(Dialog dialog, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();

        IsShowing = true;

        dialogBox.SetActive(true);

        foreach (var line in dialog.Lines)
        {
            AudioManager.Instance.PlaySfx(AudioId.UISelect);
            yield return TypeDialog(line);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.X));
        }

        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected);
        }
        // else
        // {
        //     yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.X));
        // }
        
        
        dialogBox.SetActive(false);
        IsShowing = false;
        OnDialogFinished?.Invoke();
    }

    public void HandleUpdate()
    {
       
    }

    public IEnumerator TypeDialog(string line)
    {
        dialogText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
    }
}
