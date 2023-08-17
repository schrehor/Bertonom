using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { Menu, Buying, Selling, Busy }

public class ShopController : MonoBehaviour
{
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private WalletUI walletUI;

    public event Action OnStartShopping;
    public event Action OnFinishShopping;
    
    private ShopState _state;

    public static ShopController Instance { get; private set; }

    private Inventory _inventory;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _inventory = Inventory.GetInventory();
    }

    public IEnumerator StartTrading(Merchant merchant)
    {
        OnStartShopping?.Invoke();
        yield return StartMenuState();
    }

    IEnumerator StartMenuState()
    {
        _state = ShopState.Menu;
        
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($"Nazdaar, nechces nejake drogy?", 
            waitForInput: false,
            choices: new List<string>() { "Urcite ano", "Nie ale predam ti", "Fuj ty skurveny droger vyjebany pojebany" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Buy
        }
        else if (selectedChoice == 1)
        {
            //Sell
            _state = ShopState.Selling;
            inventoryUI.gameObject.SetActive(true);
        }
        else if (selectedChoice == 2)
        {
            OnFinishShopping?.Invoke();
            yield break;
        }
    }

    public void HandleUpdate()
    {
        if (_state == ShopState.Selling)
        {
            inventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) => StartCoroutine(SellItem(selectedItem)));
        }
    }

    void OnBackFromSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMenuState());
    }

    IEnumerator SellItem(ItemBase item)
    {
        _state = ShopState.Busy;

        if (!item.IsSellabe)
        {
            yield return DialogManager.Instance.ShowDialogText("Toto ti nekupim, to je hnus");
            _state = ShopState.Selling;
            yield break;
        }
        
        walletUI.Show();
        
        var sellingPrice = Mathf.Round(item.Price / 2);
        
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($"Za tuto picovinu ti dam max {sellingPrice}, aj len kvoli tomu, ze vyzeras ako uplne hovno", 
            waitForInput: false,
            choices: new List<string>() { "Beriem, ty drzgros skurvety", "Tak to si naser do huby" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Sell
            _inventory.RemoveItem(item);
            Wallet.Instance.AddMoney(sellingPrice);
            yield return DialogManager.Instance.ShowDialogText($"Dik za obchod a skap");
        }
        
        walletUI.Close();
        
        _state = ShopState.Selling;
    }
}
