using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float moveSpeed;
    CharacterAnimator animator;

    public bool IsMoving { get; private set; }

    public CharacterAnimator Animator { get => animator; }

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
    }

    /// <summary>
    /// If the path to the moveVec is accesible, the coroutine moves the target to the given location.
    /// </summary>
    /// <param name="moveVec">Destination vector</param>
    /// <param name="OnMoveOver">Action that invokes methods after the target finished moving</param>
    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsPathClear(targetPos))
        {
            yield break;
        }

        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        //transform.position = targetPos;
        IsMoving = false;
        
        OnMoveOver?.Invoke();
    }

    /// <summary>
    /// Updates the IsMoving parameter of the animator
    /// </summary>
    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    /// <summary>
    /// Checks if the path from the caller to the given targetPos is clear.
    /// </summary>
    /// <param name="targetPos">The target position of the caller.</param>
    /// <returns>True if the path is clear</returns>
    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, 
            GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer))
        {
            return false;
        }

        return true;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.15f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Makes the character face the targetPos.
    /// </summary>
    /// <param name="targetPos">Character which we want to face.</param>
    public void LookTowards(Vector3 targetPos)
    {
        var xDiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var yDiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);
       
        if (xDiff == 0 || yDiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xDiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(yDiff, -1f, 1f);
        }
        else
        {
            Debug.LogError("Error in Look Towards: You can't ask the character to look diagonal");
        }
    }
}
