using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject itemList;
    [SerializeField] private ItemSlotUI itemSlotUI;
    
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemDescription;

    [SerializeField] private Image upArrow;
    [SerializeField] private Image downArrow;

    private const int ItemsInViewport = 8;
    
    private List<ItemBase> _availableItems;
    private List<ItemSlotUI> _slotUIList;
    private int _selectedItem;
    private float _itemSlotUIHeight;
    private RectTransform _itemListRect;

    private Action<ItemBase> _onItemSelected;
    private Action _onBack;

    private void Awake()
    {
        _itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        _itemSlotUIHeight = itemSlotUI.GetComponent<RectTransform>().rect.height;
    }

    public void Show(List<ItemBase> availableItems, Action<ItemBase> onItemSelected, Action onBack)
    {
        _availableItems = availableItems;
        _onItemSelected = onItemSelected;
        _onBack = onBack;
        
        gameObject.SetActive(true);
        UpdateItemList();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    
    public void HandleUpdate()
    {
        var prevSelectedItem = _selectedItem;
        
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _selectedItem++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _selectedItem--;
        }
        
        _selectedItem = Mathf.Clamp(_selectedItem, 0, _availableItems.Count - 1);

        if (_selectedItem != prevSelectedItem)
        {
            UpdateItemSelection();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            _onItemSelected?.Invoke(_availableItems[_selectedItem]);
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            _onBack?.Invoke();
        }
    }
    
    private void UpdateItemList()
    {
        // Clear the existing items
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        _slotUIList = new List<ItemSlotUI>();
        foreach (var item in _availableItems)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(item);
            
            _slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }
    
    void UpdateItemSelection()
    {
        _selectedItem = Mathf.Clamp(_selectedItem, 0, _availableItems.Count - 1);
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
        
        if (_availableItems.Count > 0)
        {
            var item = _availableItems[_selectedItem];
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
}
