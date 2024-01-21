using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ledge : MonoBehaviour
{
    [SerializeField] int xDir;
    [SerializeField] int yDir;

	private void Awake()
	{
		GetComponent<SpriteRenderer>().enabled = false;
	}

	public bool TryToJump(Character character, Vector2 moveDir)
    {
        if (moveDir.x == xDir && moveDir.y == yDir)
        {
            StartCoroutine(Jump(character));
            return true;
        }

        return false;
    }

    IEnumerator Jump(Character character)
    {
        GameController.Instance.PauseGame(true);
        character.Animator.IsJumping = true;

		var jumpDestination = character.transform.position + new Vector3(xDir, yDir, 0) * 2;
        yield return character.transform.DOJump(jumpDestination, 0.3f, 1, 0.5f).WaitForCompletion();

		character.Animator.IsJumping = false;
		GameController.Instance.PauseGame(false);
	}
}
