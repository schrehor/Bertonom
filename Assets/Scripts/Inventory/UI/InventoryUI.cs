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


    private List<ItemSlotUI> _slotUIList;
    private Inventory _inventory;
    private int _selecteItem = 0;

    private void Awake()
    {
        _inventory = Inventory.GetInventory();
    }

    private void Start()
    {
        UpdateItemList();
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
    }
}
