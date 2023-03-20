using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum ItemCategory { Items, Pokeball, Tms }

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<ItemSlot> slots;
    [SerializeField] private List<ItemSlot> pokeballSlots;
    [SerializeField] private List<ItemSlot> tmSlots;

    private List<List<ItemSlot>> _allSlots;
    public event Action OnUpdated;

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "ITEMS", "POKEBALLS", "TMs & HMs"
    };

    private void Awake()
    {
        _allSlots = new List<List<ItemSlot>>() {slots, pokeballSlots, tmSlots};
    }

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return _allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currentSlot = GetSlotsByCategory(categoryIndex);
        return currentSlot[itemIndex].Item;
    }
    
    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            if (!item.IsReusable)
            {
                RemoveItem(item, selectedCategory);
            }
            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count = 1)
    {
        var category = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotsByCategory(category);

        var itemSlot = currentSlot.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlot.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }
        OnUpdated?.Invoke();
    }
    
    public void RemoveItem(ItemBase item, int selectedCategory)
    {
        var currentSlot = GetSlotsByCategory(selectedCategory);
        var itemSlot = currentSlot.First(slot => slot.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0)
        {
            currentSlot.Remove(itemSlot);
        }
        
        OnUpdated?.Invoke();
    }

    ItemCategory GetCategoryFromItem(ItemBase item)
    {
        switch (item)
        {
            case RecoveryItem:
                return ItemCategory.Items;
            case PokeballItem:
                return ItemCategory.Pokeball;
            case TmItem:
                return ItemCategory.Tms;
            default:
                return ItemCategory.Items;
        }
    }
    
    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] private ItemBase item;
    [SerializeField] private int count;

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }

    public int Count
    {
        get => count;
        set => count = value;
    }
}

