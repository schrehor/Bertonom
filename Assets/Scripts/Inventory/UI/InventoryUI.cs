using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, MoveToForget, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject itemList;
    [SerializeField] private ItemSlotUI itemSlotUI;
    
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemDescription;
    [SerializeField] private Text categoryText;

    [SerializeField] private Image upArrow;
    [SerializeField] private Image downArrow;
    
    [SerializeField] private PartyScreen partyScreen;
    [SerializeField] private MoveForgettingUI moveForgettingUI;

    private Action<ItemBase> _onItemUsed;
    
    private const int ItemsInViewport = 8;

    private InventoryUIState _state;
    private List<ItemSlotUI> _slotUIList;
    private Inventory _inventory;
    private int _selectedItem;
    private RectTransform _itemListRect;
    private float _itemSlotUIHeight;
    private int _selectedCategory;
    private MoveBase _moveToLearn;
    
    private void Awake()
    {
        _inventory = Inventory.GetInventory();
        _itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();
        _itemSlotUIHeight = itemSlotUI.GetComponent<RectTransform>().rect.height;

        _inventory.OnUpdated += UpdateItemList;
    }

    private void UpdateItemList()
    {
        // Clear the existing items
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        _slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in _inventory.GetSlotsByCategory(_selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);
            
            _slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
    {
        _onItemUsed = onItemUsed;
        
        if (_state == InventoryUIState.ItemSelection)
        {
            int prevSelection = _selectedItem;
            int prevCategory = _selectedCategory;
            
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _selectedItem++;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _selectedItem--;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _selectedCategory++;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _selectedCategory--;
            }
            
            _selectedCategory = (_selectedCategory + Inventory.ItemCategories.Count) % Inventory.ItemCategories.Count;
            //_selectedCategory = Mathf.Clamp(_selectedCategory, 0, Inventory.ItemCategories.Count - 1);
            _selectedItem = Mathf.Clamp(_selectedItem, 0, _inventory.GetSlotsByCategory(_selectedCategory).Count - 1);

            if (prevCategory != _selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[_selectedCategory];
                UpdateItemList();
            }
            else if (_selectedItem != prevSelection)
            {
                UpdateItemSelection();
            }
            
            if (Input.GetKeyDown(KeyCode.X))
            {
                StartCoroutine(ItemSelected());
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                onBack?.Invoke();
            }
        }
        else if (_state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                // Use item on the selected pokemon
                StartCoroutine(UseItem());

            };
            
            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };
            
            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
        else if (_state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };
            
            moveForgettingUI.HandleMoveSelection(onMoveSelected);
        }
    }

    IEnumerator ItemSelected()
    {
        _state = InventoryUIState.Busy;
        var item = _inventory.GetItem(_selectedItem, _selectedCategory);
        
        if (GameController.Instance.State == GameState.Battle)
        {
            // In battle
            if (!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used in battle");
                _state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            // Outside battle
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be outside of battle");
                _state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        
        if (_selectedCategory == (int)ItemCategory.Pokeball)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();

            if (item is TmItem)
            {
                // Show if TM is usable
                partyScreen.ShowIfTmUsable(item as TmItem);
            }
        }
    }

    IEnumerator UseItem()
    {
        _state = InventoryUIState.Busy;

        yield return HandleTmItems();
        
        var usedItem = _inventory.UseItem(_selectedItem, partyScreen.SelectedMember, _selectedCategory);
        if (usedItem != null)
        {
            if (usedItem is RecoveryItem)
            {
                yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}");
            }
            _onItemUsed?.Invoke(usedItem);
        }
        else
        {
            if (_selectedItem == (int)ItemCategory.Items)
            {
                yield return DialogManager.Instance.ShowDialogText($"It won't have any effect");
            }
        }
        
        ClosePartyScreen();
    }

    IEnumerator HandleTmItems()
    {
        var tmItem = _inventory.GetItem(_selectedItem, _selectedCategory) as TmItem;
        if (tmItem == null)
        {
            yield break;
        }

        var pokemon = partyScreen.SelectedMember;

        if (pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} already learned {tmItem.Move.Name}");
            yield break;
        }

        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} can't learn {tmItem.Move.Name}");
            yield break;
        }

        if (pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} learned {tmItem.Move.Name}");
        }
        else
        {
            // Forget a move
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is trying to learn {tmItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"But it cannot learn more than {PokemonBase.MaxNumOfMoves} moves");
            yield return ChooseMoveToForget(pokemon, tmItem.Move);
            yield return new WaitUntil(() => _state != InventoryUIState.MoveToForget);
        }
    }
    
    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        _state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"Choose a move that you want to forget", true, false);
        moveForgettingUI.gameObject.SetActive(true);
        moveForgettingUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        _moveToLearn = newMove;

        _state = InventoryUIState.MoveToForget;
    }
    
    void UpdateItemSelection()
    {
        var slots = _inventory.GetSlotsByCategory(_selectedCategory);
        
        _selectedItem = Mathf.Clamp(_selectedItem, 0, slots.Count - 1);
        for (int i = 0; i < _slotUIList.Count; i++)
        {
            if (i == _selectedItem)
            {
                _slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                _slotUIList[i].NameText.color = Color.black;
            }
        }
        
        if (slots.Count > 0)
        {
            var item = slots[_selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }
        
        HandleScrolling();
    }
    
    private void HandleScrolling()
    {
        if (_slotUIList.Count <= ItemsInViewport)
        {
            return;
        }
        
        float scrollPos = Mathf.Clamp(_selectedItem - ItemsInViewport / 2, 0, _selectedItem) * _itemSlotUIHeight;
        _itemListRect.localPosition = new Vector2(_itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = _selectedItem > ItemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);
        
        bool showDownArrow = _selectedItem + ItemsInViewport / 2 < _slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void ResetSelection()
    {
        _selectedItem = 0;
        
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }
    
    void OpenPartyScreen()
    {
        _state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }
    
    void ClosePartyScreen()
    {
        _state = InventoryUIState.ItemSelection;
        
        partyScreen.ClearMemberSlotMessages();
        partyScreen.gameObject.SetActive(false);
    }

    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        var pokemon = partyScreen.SelectedMember;
                
        DialogManager.Instance.CloseDialog();
        moveForgettingUI.gameObject.SetActive(false);
        if (moveIndex == PokemonBase.MaxNumOfMoves)
        {
            // Don't learn new move
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} did not learn {_moveToLearn.Name}");
        }
        else
        {
            // Forget and learn new move
            var selectedMove = pokemon.Moves[moveIndex].Base.Name;
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} forgot {selectedMove} and learned {_moveToLearn.Name}");
            pokemon.Moves[moveIndex] = new Move(_moveToLearn);
        }

        _moveToLearn = null;
        _state = InventoryUIState.ItemSelection;
    }
}
