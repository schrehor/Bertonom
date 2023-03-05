using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }
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

    private Action<ItemBase> _onItemUsed;
    
    private const int ItemsInViewport = 8;

    private InventoryUIState _state;
    private List<ItemSlotUI> _slotUIList;
    private Inventory _inventory;
    private int _selectedItem;
    private RectTransform _itemListRect;
    private float _itemSlotUIHeight;
    private int _selectedCategory;

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
                ItemSelected();
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
    }

    void ItemSelected()
    {
        if (_selectedCategory == (int)ItemCategory.Pokeball)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
        }
    }

    IEnumerator UseItem()
    {
        _state = InventoryUIState.Busy;
        
        var usedItem = _inventory.UseItem(_selectedItem, partyScreen.SelectedMember, _selectedCategory);
        if (usedItem != null)
        {
            yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}");
            _onItemUsed?.Invoke(usedItem);
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"It won't have any effect");
        }
        
        ClosePartyScreen();
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
        partyScreen.gameObject.SetActive(false);
    }
}
