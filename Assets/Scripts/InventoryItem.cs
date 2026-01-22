using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [SerializeField] private Image itemPreview;

    public void Bind(UniqueItem_SO item)
    {
        if (itemPreview == null)
            return;

        if (item == null || item.icon == null)
        {
            itemPreview.enabled = false;
            itemPreview.sprite = null;
            return;
        }

        itemPreview.enabled = true;
        itemPreview.sprite = item.icon;
    }
}
