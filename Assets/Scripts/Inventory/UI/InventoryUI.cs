using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject itemList;
    [SerializeField] private ItemSlotUI itemSlotUI;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemDescription;
    [SerializeField] private Image upArrow;
    [SerializeField] private Image downArrow;


    private const int ItemsInViewport = 8;
    
    private List<ItemSlotUI> _slotUIList;
    private Inventory _inventory;
    private int _selecteItem = 0;
    private RectTransform _itemListRect;
    private float _itemSlotUIHeight;

    private void Awake()
    {
        _inventory = Inventory.GetInventory();
        _itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();
        _itemSlotUIHeight = itemSlotUI.GetComponent<RectTransform>().rect.height;
    }

    private void UpdateItemList()
    {
        // Clear the existing items
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        _slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in _inventory.Slots)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);
            
            _slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandeUpdate(Action onBack)
    {
        int prevSelection = _selecteItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _selecteItem++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _selecteItem--;
        }

        _selecteItem = Mathf.Clamp(_selecteItem, 0, _inventory.Slots.Count - 1);

        if (_selecteItem != prevSelection)
        {
            UpdateItemSelection();
        }
        
        if (Input.GetKeyDown(KeyCode.Z))
        {
            onBack?.Invoke();
        }
    }
    
    void UpdateItemSelection()
    {
        for (int i = 0; i < _slotUIList.Count; i++)
        {
            if (i == _selecteItem)
            {
                _slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                _slotUIList[i].NameText.color = Color.black;
            }
        }

        var item = _inventory.Slots[_selecteItem].Item;
        itemIcon.sprite = item.Icon;
        itemDescription.text = item.Description;

        HandleScrolling();
    }
    
    private void HandleScrolling()
    {
        float scrollPos = Mathf.Clamp(_selecteItem - ItemsInViewport / 2, 0, _selecteItem) * _itemSlotUIHeight;
        _itemListRect.localPosition = new Vector2(_itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = _selecteItem > ItemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);
        
        bool showDownArrow = _selecteItem + ItemsInViewport / 2 < _slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }
}
