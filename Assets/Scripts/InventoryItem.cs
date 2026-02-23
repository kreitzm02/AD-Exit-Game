using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image itemPreview;

    public event Action<UniqueItem_SO> Clicked;
    public event Action<UniqueItem_SO, PointerEventData> DragStarted;
    public event Action<PointerEventData> Dragging;
    public event Action<UniqueItem_SO, PointerEventData> DragEnded;

    private UniqueItem_SO boundItem;

    public void Bind(UniqueItem_SO item)
    {
        boundItem = item;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (boundItem == null) return;
        Clicked?.Invoke(boundItem);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (boundItem == null) return;
        DragStarted?.Invoke(boundItem, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Dragging?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (boundItem == null) return;
        DragEnded?.Invoke(boundItem, eventData);
    }
}
