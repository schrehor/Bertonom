using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable
{
    [SerializeField] private ItemBase item;
    [SerializeField] private int count = 1;
    [SerializeField] private Dialog dialog;

    private bool _used = false;
    
    public IEnumerator GiveItem(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);
        player.GetComponent<Inventory>().AddItem(item, count);
        _used = true;

		AudioManager.Instance.PlaySfx(AudioId.ItemObtained, pauseMusic: true);

		string dialogText = $"{player.Name} received {item.Name}";
        if (count > 1)
        {
            dialogText = $"{player.Name} received {count} {item.Name}s";
        }
        
        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return item != null && !_used && count > 0;
    }

    public object CaptureState()
    {
        return _used;
    }

    public void RestoreState(object state)
    {
        _used = (bool) state;
    }
}
