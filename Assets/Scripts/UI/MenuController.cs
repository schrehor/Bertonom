using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;

    public event Action<int> onMenuSelected;
    public event Action onBack;

    List<Text> menuItems;
    int selecteItem = 0;

    private void Awake()
    {
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
    }

    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
    }

    public void HandleUpdate()
    {
        int prevSelection = selecteItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selecteItem++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selecteItem--;
        }

        selecteItem = Mathf.Clamp(selecteItem, 0, menuItems.Count - 1);

        if (selecteItem != prevSelection)
        {
            UpdateItemSelection();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            onMenuSelected?.Invoke(selecteItem);
            CloseMenu();
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            onBack?.Invoke();
            CloseMenu();
        }
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (i == selecteItem)
            {
                menuItems[i].color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                menuItems[i].color = Color.black;
            }
        }
    }
}
