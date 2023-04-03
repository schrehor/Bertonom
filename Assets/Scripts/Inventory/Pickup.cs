using System.Collections;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable
{
    [SerializeField] private ItemBase item;
    public bool Used { get; set; }
    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);

            Used = true;

            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            string playerName = initiator.GetComponent<PlayerController>().Name;
            
            yield return DialogManager.Instance.ShowDialogText($"{playerName} found {item.Name}");
        }
    }
}
