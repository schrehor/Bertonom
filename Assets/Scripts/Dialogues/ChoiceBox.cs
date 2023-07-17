using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] private ChoiceText choiceTextPrefab;

    bool choiceSelected;

    private List<ChoiceText> _choiceTexts;
    private int _currentChoice;

    public IEnumerator ShowChoices(List<string> choices, Action<int> onChoiceSelected)
    {
        choiceSelected = false;
        _currentChoice = 0;
        
        gameObject.SetActive(true);
        
        // Delete all children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        _choiceTexts = new List<ChoiceText>();
        foreach (var choice in choices)
        {
            var choiceTextObj = Instantiate(choiceTextPrefab, transform);
            choiceTextObj.TextField.text = choice;
            _choiceTexts.Add(choiceTextObj);
        }
        
        yield return new WaitUntil(() => choiceSelected);
        
        onChoiceSelected?.Invoke(_currentChoice);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _currentChoice++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _currentChoice--;
        }

        _currentChoice = Mathf.Clamp(_currentChoice, 0, _choiceTexts.Count - 1);

        for (int i = 0; i < _choiceTexts.Count; i++)
        {
            _choiceTexts[i].SetSelected(i == _currentChoice);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            choiceSelected = true;
        }
    }
}
