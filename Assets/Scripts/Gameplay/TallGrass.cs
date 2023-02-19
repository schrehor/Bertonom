using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TallGrass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = true;
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            GameController.Instance.StartBattle();
        }
    }
}
