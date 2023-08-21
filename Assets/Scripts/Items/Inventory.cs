using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum ItemCategory { Items, Pokeball, Tms }

public class Inventory : MonoBehaviour, ISavable
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
                RemoveItem(item);
            }
            return item;
        }

        return null;
    }

    ItemSlot GetItemSlot(ItemBase item)
    {
        var category = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotsByCategory(category);

        return currentSlot.FirstOrDefault(slot => slot.Item == item);
    }
    
    ItemSlot GetItemSlot(ItemBase item, out List<ItemSlot> currentSlot)
    {
        var category = (int)GetCategoryFromItem(item);
        currentSlot = GetSlotsByCategory(category);

        return currentSlot.FirstOrDefault(slot => slot.Item == item);
    }
    
    public void AddItem(ItemBase item, int count = 1)
    {
        var itemSlot = GetItemSlot(item, out var currentSlot);
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
    
    public void RemoveItem(ItemBase item, int countToRemove = 1)
    {
        var selectedCategory = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotsByCategory(selectedCategory);
        var itemSlot = currentSlot.First(slot => slot.Item == item);
        itemSlot.Count -= countToRemove;
        if (itemSlot.Count == 0)
        {
            currentSlot.Remove(itemSlot);
        }
        
        OnUpdated?.Invoke();
    }
    
    public int GetItemCount(ItemBase item)
    {
        var itemSlot = GetItemSlot(item);

        if (itemSlot != null)
        {
            return itemSlot.Count;
        }

        return 0;
    }

    public bool HasItem(ItemBase item)
    {
        var selectedCategory = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotsByCategory(selectedCategory);
        
        return currentSlot.Exists(slot => slot.Item == item);
    }
    
    ItemCategory GetCategoryFromItem(ItemBase item)
    {
        switch (item)
        {
            case RecoveryItem:
            case EvolutionItem:
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

    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            items = slots.Select(i => i.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();

        _allSlots = new List<List<ItemSlot>>() {slots, pokeballSlots, tmSlots};
        
        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] private ItemBase item;
    [SerializeField] private int count;

    public ItemSlot(){}
    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }
    
    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.name,
            count = count
        };

        return saveData;
    }

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

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> pokeballs;
    public List<ItemSaveData> tms;
}