using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase _base)
    {
        Base = _base;
    }
    
    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;

        yield return DialogManager.Instance.ShowDialog(Base.StartDialog);
        
        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public IEnumerator CompleteQuest(Transform playerTransform)
    {
        Status = QuestStatus.Completed;
     
        yield return DialogManager.Instance.ShowDialog(Base.CompletedDialog);

        var inventory = Inventory.GetInventory();
        if (Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }

        if (Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);

            var playerName = playerTransform.GetComponent<PlayerController>().Name;
            yield return DialogManager.Instance.ShowDialogText($"{playerName} received {Base.RewardItem.Name}!");
        }
        
        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public bool CanBeCompleted()
    {
        if (Base.RequiredItem != null)
        {
            var inventory = Inventory.GetInventory();
            if (!inventory.HasItem(Base.RequiredItem))
            {
                return false;
            }
        }
        return true;
    }
}

public enum QuestStatus {None, Started, Completed}