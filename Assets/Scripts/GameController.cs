using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Menu, PartyScreen, Bag, Cutscene, Paused, Evolution, Shop }
public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private PartyScreen partyScreen;
    [SerializeField] private InventoryUI inventoryUI;

    private GameState _state;
    private GameState _prevState;
    private GameState _stateBeforeEvolution;
    private TrainerController _trainer;
    private MenuController _menuController;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        _menuController = GetComponent<MenuController>();

        // TODO: remove comments after finishing the game to disable mouse
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;
        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            _prevState = _state;
            _state = GameState.Dialog;
        };

        DialogManager.Instance.OnDialogFinished += () =>
        {
            if (_state == GameState.Dialog)
            {
                _state = _prevState;
            }
        };

        _menuController.onBack += () =>
        {
            _state = GameState.FreeRoam;
        };

        _menuController.onMenuSelected += OnMenuSelected;

        EvolutionManager.i.OnStartEvolution += () =>
        {
            _stateBeforeEvolution = _state;
            _state = GameState.Evolution;
        };
        EvolutionManager.i.OnEndEvolution += () =>
        {
            partyScreen.SetPartyData();
            _state = _stateBeforeEvolution;
        };
        
        ShopController.Instance.OnStartShopping += () => _state = GameState.Shop; 
        ShopController.Instance.OnFinishShopping += () => _state = GameState.FreeRoam; 
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            _prevState = _state;
            _state = GameState.Paused;
        }
        else
        {
            _state = _prevState;
        }
    }

    void EndBattle(bool won)
    {
        if (_trainer != null && won == true)
        {
            _trainer.BattleLost();
            _trainer = null;
        }
        
        partyScreen.SetPartyData();

        _state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        var playerParty = playerController.GetComponent<PokemonParty>();
        StartCoroutine(playerParty.CheckForEvolutions());
    }

    public void StartBattle()
    {
        _state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }

    public void StartTrainerBattle(TrainerController trainer)
    {
        _state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this._trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
        _state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    private void Update()
    {
        if (_state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.S))
            {
                _menuController.OpenMenu();
                _state = GameState.Menu;
            }
        }
        else if (_state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (_state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (_state == GameState.Menu)
        {
            _menuController.HandleUpdate();
        }
        else if (_state == GameState.PartyScreen)
        {
            Action onSeleced = () =>
            {
                // TODO: Go to Summary screen
            };

            Action onBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                _state = GameState.FreeRoam;
            };

            partyScreen.HandleUpdate(onSeleced, onBack);
        }
        else if (_state == GameState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                _state = GameState.FreeRoam;
            };

            inventoryUI.HandleUpdate(onBack);
        }
        else if (_state == GameState.Shop)
        {
            ShopController.Instance.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            // Pokemon
            partyScreen.gameObject.SetActive(true);
            _state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            // Bag
            inventoryUI.gameObject.SetActive(true);
            _state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            // Save
            SavingSystem.i.Save("saveSlot1");
            _state = GameState.FreeRoam;
        }
        else if (selectedItem == 3)
        {
            // Load
            SavingSystem.i.Load("saveSlot1");
            _state = GameState.FreeRoam;
        }
    }

    public GameState State => _state;
}
