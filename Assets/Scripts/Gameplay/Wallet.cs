using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [SerializeField] private float money;
    
    public float Money => money;
    
    public void AddMoney(float amount)
    {
        money += amount;
    }

    public void TakeMoney(float amount)
    {
        money -= amount;
    }
}
