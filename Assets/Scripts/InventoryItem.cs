using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [SerializeField] private Image itemPreview;

    private void AssignImage(Sprite itemSprite)
    {
        itemPreview.sprite = itemSprite;
    }
}
