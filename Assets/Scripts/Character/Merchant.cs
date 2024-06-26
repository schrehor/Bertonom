using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : MonoBehaviour
{
    [SerializeField] private List<ItemBase> availableItems;

    public List<ItemBase> AvailableItems => availableItems;
    
    public IEnumerator Trade()
    {
        yield return ShopController.Instance.StartTrading(this);
    }
}
