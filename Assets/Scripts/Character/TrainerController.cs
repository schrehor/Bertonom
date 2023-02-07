using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    [SerializeField] Dialog dialog;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        // Show Exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        // Walk towards player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        // Show dialog before battle
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
        {
            Debug.Log("Idem ta vykuchat");
        }));
    }

    public void SetFovRotation(FacingDirection direction)
    {
        float angle = 0f;
        switch (direction)
        {
            case FacingDirection.Up:
                angle = 180f;
                break;
            case FacingDirection.Left:
                angle = 270f;
                break;
            case FacingDirection.Right:
                angle = 90f;
                break;
            default:
                break;
        }

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }
}
