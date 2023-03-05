using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget, BattleOver } 
public enum BattleAction { Move, SwitchPokemon, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleUnit enemyUnit;
    [SerializeField] private BattleDialogBox dialogBox;
    [SerializeField] private PartyScreen partyScreen;
    [SerializeField] private Image playerImage;
    [SerializeField] private Image trainerImage;
    [SerializeField] private GameObject pokeballSprite;
    [SerializeField] private MoveForgettingUI moveForgettingUI;
    [SerializeField] private InventoryUI inventoryUI;

    public event Action<bool> OnBattleOver;

    private BattleState _state;
    private int _currentAction;
    private int _currentMove;
    private bool _aboutToUseChoice = true;
    private bool _isTrainerBattle;

    private PlayerController _player;
    private TrainerController _trainer;
    private PokemonParty _playerParty;
    private PokemonParty _trainerParty;
    private Pokemon _wildPokemon;

    private int _escapeAttempts;
    private MoveBase _moveToLearn;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        _isTrainerBattle = false;
        this._playerParty = playerParty;
        this._wildPokemon = wildPokemon;
        _player = playerParty.GetComponent<PlayerController>();

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this._playerParty = playerParty;
        this._trainerParty = trainerParty;

        _isTrainerBattle = true;
        _player = playerParty.GetComponent<PlayerController>();
        _trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        // Hides the battle hud
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!_isTrainerBattle)
        {
            // Wild pokemon battle
            playerUnit.Setup(_playerParty.GetHealthyPokemon());
            enemyUnit.Setup(_wildPokemon);
            dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared.");
        }
        else
        {
            // Trainer battle

            // Hide pokemon and show trainers
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = _player.Sprite;
            trainerImage.sprite = _trainer.Sprite;

            yield return dialogBox.TypeDialog($"{_trainer.Name} ti ide dat napicu");

            // Send out first pokemon
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = _trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{_trainer.Name} sent our {enemyPokemon.Base.Name}");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = _playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
            dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);
        }

        _escapeAttempts = 0;
        partyScreen.Init();
             
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        _state = BattleState.BattleOver;
        _playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        OnBattleOver?.Invoke(won);
    }

    void ActionSelection()
    {
        _state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    void MoveSelection()
    {
        _state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        _state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[_currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            // Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
               playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }
            
            var firstUnit = playerGoesFirst ? playerUnit : enemyUnit;
            var secondUnit = playerGoesFirst ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            // First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (_state == BattleState.BattleOver)
            {
                yield break;
            }

            if (secondPokemon.HP > 0)
            {
                // Second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (_state == BattleState.BattleOver)
                {
                    yield break;
                }
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                _state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                // This is handled from item screen, so do nothing and skip to enemy move
                dialogBox.EnableActionSelector(false);
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (_state == BattleState.BattleOver)
            {
                yield break;
            }
        }

        if (_state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        if (!canRunMove)
        {
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        
        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        if (CheckIfMovesHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondaryEffect in move.Base.SecondaryEffects)
                {
                    var random = UnityEngine.Random.Range(1, 101);
                    if (random <= secondaryEffect.Chance)
                    {
                        yield return RunMoveEffects(secondaryEffect, sourceUnit.Pokemon, targetUnit.Pokemon, secondaryEffect.Target);
                    }
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed");
        }
    } 
    
    // IEnumerable CheckFainted(BattleUnit unit)
    // {
    //     if (unit.Pokemon.HP <= 0)
    //     {
    //         yield return dialogBox.TypeDialog($"{unit.Pokemon.Base.PkmName} fainted");
    //         unit.PlayFaintAnimation();
    //
    //         yield return new WaitForSeconds(2f);
    //
    //         ChechForBattleOver(unit);
    //     }
    // }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        // Stat boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoost(effects.Boosts);
            }
            else
            {
                target.ApplyBoost(effects.Boosts);
            }
        }

        // Status conditions
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);    
        }

        // Volatile status conditions
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }


        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (_state == BattleState.BattleOver)
        {
            yield break;
        }
        
        yield return new WaitUntil(() => _state == BattleState.RunningTurn);

        // Statuses will hurt pokemon after turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.WaitForHPUpdate();

        // Check if status damage is fatal
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => _state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMovesHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        float[] boostValues = { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} fainted");
        faintedUnit.PlayFaintAnimation();

        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            // Exp Gain
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = (_isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();

            // Check for level up
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                playerUnit.Pokemon.BoostStatsAfterLevelUp();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} grew to level {playerUnit.Pokemon.Level}");

                // Try to learn new move
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtLevel();
                if (newMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} learned {newMove.MoveBase.Name}");
                        dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        // Forget a move
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} is trying to learn {newMove.MoveBase.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {PokemonBase.MaxNumOfMoves} moves");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.MoveBase);
                        yield return new WaitUntil(() => _state != BattleState.MoveToForget);

                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = _playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            if (!_isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextPokemon = _trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    StartCoroutine(AboutToUse(nextPokemon));
                }
                else
                {
                    BattleOver(true);
                }
            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("A critical hit!");
        }

        if (damageDetails.TypeEffectivness > 1f)
        {
            yield return dialogBox.TypeDialog("It's super effective!");
        }
        else if (damageDetails.TypeEffectivness < 1f && damageDetails.TypeEffectivness != 0f)
        {
            yield return dialogBox.TypeDialog("It's not very effective!");
        }
        else if (damageDetails.TypeEffectivness == 0f)
        {
            yield return dialogBox.TypeDialog($"It has no effect!");
        }
    }

    int SelectOption(int selectedOption)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedOption += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedOption -= 2;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedOption--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedOption++;
        }

        return selectedOption;
    }

    public void HandleUpdate()
    {
        if (_state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (_state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (_state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (_state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                _state = BattleState.ActionSelection;
            };

            Action<ItemBase> onItemUsed = useItem =>
            {
                StartCoroutine(OnItemUsed(useItem));
            };
            
            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (_state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (_state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = moveIndex =>
            {
                moveForgettingUI.gameObject.SetActive(false);

                if (moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    // Don't learn new move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} did not learn {_moveToLearn.Name}"));
                }
                else
                {
                    // Forget and learn new move
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base.Name;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} forgot {selectedMove} and learned {_moveToLearn.Name}"));
                    playerUnit.Pokemon.Moves[moveIndex] = new Move(_moveToLearn);
                }

                _moveToLearn = null;
                _state = BattleState.RunningTurn;
            };

            moveForgettingUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleMoveSelection()
    {
        _currentMove = SelectOption(_currentMove);

        _currentMove = Mathf.Clamp(_currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdatMoveSelection(_currentMove, playerUnit.Pokemon.Moves[_currentMove]);

        if (Input.GetKeyDown(KeyCode.X))
        {
            var move = playerUnit.Pokemon.Moves[_currentMove];
            if (move.PP == 0)
            {
                return;
            }

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandleActionSelection()
    {
        _currentAction = SelectOption(_currentAction);

        _currentAction = Mathf.Clamp(_currentAction, 0, 3);

        dialogBox.UpdateActionSelection(_currentAction);

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (_currentAction == 0)
            {
                // Fight
                MoveSelection();
            } 
            else if (_currentAction == 1)
            {
                // Bag
                OpenBag();
            }
            else if (_currentAction == 2)
            {
                // Pokemon
                OpenPartyScreen();
            }
            else if (_currentAction == 3)
            {
                // Run
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void OpenBag()
    {
        _state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }
    
    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = _state;
        _state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;

            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted pokemon");
                return;
            }

            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch with the same pokemon");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                _state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a pokemon to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
            {
                ActionSelection();
            }

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            _aboutToUseChoice = !_aboutToUseChoice;
        }

        dialogBox.UpdateChoiceBox(_aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            if (_aboutToUseChoice)
            {
                // Yes option
                OpenPartyScreen();
            }
            else
            {
                // No option
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {
        playerUnit.Pokemon.CureVolatileStatus();

        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMovesNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        if (isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerPokemon());
        }
        else
        {
            _state = BattleState.RunningTurn;
        }
    }

    IEnumerator SendNextTrainerPokemon()
    {
        _state = BattleState.Busy;

        var nextPokemon = _trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{_trainer.Name} sent out {nextPokemon.Base.Name}");

        _state = BattleState.RunningTurn;
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        _state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{_trainer.Name} is about to use {newPokemon.Base.Name}. Do you want to change your pokemon?");

        _state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        _state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move that you want to forget");
        moveForgettingUI.gameObject.SetActive(true);
        moveForgettingUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        _moveToLearn = newMove;

        _state = BattleState.MoveToForget;
    }

    IEnumerator OnItemUsed(ItemBase useItem)
    {
        _state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (useItem is PokeballItem)
        {
            yield return ThrowPokeball((PokeballItem)useItem);
        }
                
        StartCoroutine(RunTurns(BattleAction.UseItem));
    }
    
    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        _state = BattleState.Busy;

        if (_isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You cannot steal trainers pokemon!");
            _state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{_player.Name} used {pokeballItem.Name.ToUpper()}!");

        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        // Animations
        var enemyPosition = enemyUnit.transform.position;
        yield return pokeball.transform.DOJump(enemyPosition + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyPosition.y - 0.5f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon, pokeballItem);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // Pokemon is caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} was caught");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            _playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} has been added to your party");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            // Pokemon broke out
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} broke free");
            }
            else
            {
                yield return dialogBox.TypeDialog($"Almost caught it");
            }

            Destroy(pokeball);
            _state = BattleState.RunningTurn;
        }
    }

    int TryToCatchPokemon(Pokemon pokemon, PokeballItem pokeball)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * pokeball.CatchRateModifier *
            ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

        if (a >= 255)
        {
            return 4;
        }

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;

        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }

            shakeCount++;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        _state = BattleState.Busy;

        if (_isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You cannot run from trainer battles!");
            _state = BattleState.RunningTurn;
            yield break;
        }

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (playerSpeed > enemySpeed)
        {
            yield return dialogBox.TypeDialog($"Run away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * ++_escapeAttempts;
            f %= 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Run away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
                _state = BattleState.RunningTurn;
            }
        }
    }
}
