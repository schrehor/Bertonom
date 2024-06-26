using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string _name;
    [SerializeField] Sprite sprite;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;

    [SerializeField] private AudioClip trainerAppearsClip;


    // state
    bool battleLost = false;

    Character character;

    public string Name { get => _name; }
    public Sprite Sprite { get => sprite; }

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            AudioManager.Instance.PlayMusic(trainerAppearsClip);
            yield return DialogManager.Instance.ShowDialog(dialog);
            GameController.Instance.StartTrainerBattle(this);
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialogAfterBattle);
        }
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        AudioManager.Instance.PlayMusic(trainerAppearsClip);
        
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
        yield return DialogManager.Instance.ShowDialog(dialog);
        
        GameController.Instance.StartTrainerBattle(this);
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
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

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;

        if (battleLost)
        {
            fov.gameObject.SetActive(false);
        }
    }
}
