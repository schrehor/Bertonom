using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] private Dialog dialog;

    public void OnPlayerTriggered(PlayerController player)
    {
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
    }
}
