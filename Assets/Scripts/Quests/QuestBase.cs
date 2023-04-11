using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Create a new quest")]
public class QuestBase : ScriptableObject
{
    [SerializeField] private new string name;
    [TextArea][SerializeField] private string description;

    [SerializeField] private Dialog startDialog;
    [SerializeField] private Dialog inProgressDialog;
    [SerializeField] private Dialog completedDialog;

    [SerializeField] private ItemBase requiredItem;
    [SerializeField] private ItemBase rewardItem;

    public string Name => name;
    public string Description => description;

    public Dialog StartDialog => startDialog;
    public Dialog InProgressDialog => inProgressDialog?.Lines?.Count > 0 ? inProgressDialog : startDialog;
    public Dialog CompletedDialog => completedDialog;

    public ItemBase RequiredItem => requiredItem;
    public ItemBase RewardItem => rewardItem;
}
