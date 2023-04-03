using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour
{
    [SerializeField] private ItemBase item;
    [SerializeField] private Dialog dialog;

    private bool _used = false;
    
    public IEnumerator GiveItem(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);
        player.GetComponent<Inventory>().AddItem(item);
        _used = true;
        yield return DialogManager.Instance.ShowDialogText($"Player received {item.Name}");
    }

    public bool CanBeGiven()
    {
        return item != null && !_used;
    }
}
