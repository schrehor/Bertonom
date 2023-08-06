using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] private Dialog dialog;
    
    [Header("Quests")]
    [SerializeField] private QuestBase questToStart;
    [SerializeField] private QuestBase questToComplete;
    
    [Header("Movement")]
    [SerializeField] private List<Vector2> movementPattern;
    [SerializeField] private float timeBetweenPattern;

    private float _idleTimer = 0f;
    private NPCState _state;
    private int _currentPattern = 0;
    private Quest _activeQuest;

    private Character _character;
    private ItemGiver _itemGiver;
    private PokemonGiver _pokemonGiver;
    private Healer _healer;
    private Merchant _merchant;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _itemGiver = GetComponent<ItemGiver>();
        _pokemonGiver = GetComponent<PokemonGiver>();
        _healer = GetComponent<Healer>();
        _merchant = GetComponent<Merchant>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (_state != NPCState.Idle) yield break;
        _state = NPCState.Dialog;
        _character.LookTowards(initiator.position);

        if (questToComplete != null)
        {
            var quest = new Quest(questToComplete);
            yield return quest.CompleteQuest(initiator);
            questToComplete = null;
            
            Debug.Log($"{quest.Base.Name} completed");
        }
        
        if (_itemGiver != null && _itemGiver.CanBeGiven())
        {
            yield return _itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
        }
        else if (_pokemonGiver != null && _pokemonGiver.CanBeGiven())
        {
            yield return _pokemonGiver.GivePokemon(initiator.GetComponent<PlayerController>());
        }
        else if (questToStart != null)
        {
            _activeQuest = new Quest(questToStart);
            yield return _activeQuest.StartQuest();
            questToStart = null;
            
            if (_activeQuest.CanBeCompleted())
            {
                yield return _activeQuest.CompleteQuest(initiator);
                _activeQuest = null;
            }
        }
        else if (_activeQuest != null)
        {
            if (_activeQuest.CanBeCompleted())
            {
                yield return _activeQuest.CompleteQuest(initiator);
                _activeQuest = null;
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(_activeQuest.Base.InProgressDialog);
            }
        }
        else if (_healer != null)
        {
            yield return _healer.Heal(initiator, dialog);
        }
        else if (_merchant != null)
        {
            yield return _merchant.Trade();
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialog);
        }
        
            
        _idleTimer = 0f;
        _state = NPCState.Idle;
    }

    private void Update()
    {
        if (_state == NPCState.Idle)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer > timeBetweenPattern)
            {
                _idleTimer = 0f;
                if (movementPattern.Count > 0)
                {
                    StartCoroutine(Walk());
                }
            }
        }

        _character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        _state = NPCState.Walking;

        var oldPos = transform.position;

        yield return _character.Move(movementPattern[_currentPattern]);

        if (transform.position != oldPos)
        {
            _currentPattern = (_currentPattern + 1) % movementPattern.Count;
        }

        _state = NPCState.Idle;

    }

    [ContextMenu("Show Path")]
    void ShowPath()
    {
        var pos = new Vector2(transform.position.x, transform.position.y);
        var index = 0;

        var colours = new List<Color>()
        {
            Color.red,
            Color.green,
            Color.blue
        };

        foreach (Vector2 path in movementPattern)
        {
            Vector2 newPosRef = movementPattern[index];

            if (newPosRef.x == 0)
            {
                newPosRef.y *= 2f;
            }
            else if (newPosRef.y == 0)
            {
                newPosRef.x *= 2f;
            }

            Debug.DrawLine(pos, pos + newPosRef, colours[index % 3], 2f);

            index += 1;
            pos += newPosRef;
        }
    }

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();
        
        saveData.activeQuest = _activeQuest?.GetSaveData();
        if (questToStart != null)
        {
            saveData.questToStart = (new Quest(questToStart)).GetSaveData();
        }
        
        if (questToComplete != null)
        {
            saveData.questToComplete = (new Quest(questToComplete)).GetSaveData();
        }

        return saveData;
    }

    public void RestoreState(object state)
    {
        if (state is NPCQuestSaveData saveData)
        {
            _activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;
            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}

[System.Serializable]
public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

public enum NPCState { Idle, Walking, Dialog }