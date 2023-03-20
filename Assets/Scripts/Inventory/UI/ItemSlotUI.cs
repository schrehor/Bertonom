using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text countText;
    
    public Text NameText => nameText;

    public Text CountText => countText;
    
    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.Name;
        countText.text = $"X {itemSlot.Count}";
    }
}
