using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public void HandeUpdate(Action onBack)
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            onBack?.Invoke();
        }
    }
}
