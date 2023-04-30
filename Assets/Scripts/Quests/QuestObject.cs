using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] private QuestBase questToCheck;
    [SerializeField] private ObjectActions onStart;
    [SerializeField] private ObjectActions onComplete;

    private QuestList _questList;

    private void Start()
    {
        _questList = QuestList.GetQuestList();
        _questList.OnUpdated += UpdateObjectStatus;
        
        UpdateObjectStatus();
    }

    private void OnDestroy()
    {
        _questList.OnUpdated -= UpdateObjectStatus;
    }

    public void UpdateObjectStatus()
    {
        if (onStart != ObjectActions.DoNothing && _questList.IsStarted(questToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onStart == ObjectActions.Enable)
                {
                    child.gameObject.SetActive(true);
                    
                    var savableEntity = child.GetComponent<SavableEntity>();
                    if (savableEntity != null)
                    {
                        SavingSystem.i.RestoreEntity(savableEntity);
                    }
                }
                else if (onStart == ObjectActions.Disable)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        if (onComplete != ObjectActions.DoNothing && _questList.IsCompleted(questToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onComplete == ObjectActions.Enable)
                {
                    child.gameObject.SetActive(true);
                    
                    var savableEntity = child.GetComponent<SavableEntity>();
                    if (savableEntity != null)
                    {
                        SavingSystem.i.RestoreEntity(savableEntity);
                    }
                }
                else if (onComplete == ObjectActions.Disable)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}

public enum ObjectActions { DoNothing, Enable, Disable }
