using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] private ChoiceText choiceTextPrefab;

    bool choiceSelected = false;
    
    public IEnumerator ShowChoices(List<string> choices)
    {
        choiceSelected = false;
        
        gameObject.SetActive(true);
        
        // Delete all children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var choice in choices)
        {
            var choiceTextObj = Instantiate(choiceTextPrefab, transform);
            choiceTextObj.TextField.text = choice;
        }
        
        yield return new WaitUntil(() => choiceSelected);
    }
}
