using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventoryItem itemSlotPrefab;
    [SerializeField] private Transform contentRoot;

    [Header("Details UI")]
    [SerializeField] private InventoryItemDetailsUI detailsUI;

    [Header("Collapse")]
    [SerializeField] private InventoryCollapse inventoryCollapse;

    [Header("Drag Ghost")]
    [SerializeField] private Canvas rootCanvas; 
    [SerializeField] private Image dragGhostImage; 
    [SerializeField] private bool enableDragToWorldZones = true;

    [Header("Optional")]
    [SerializeField] private bool refreshOnEnable = true;

    private UniqueItem_SO currentlyDraggedItem;
    private DragItemZone currentHoverZone;

    private void Awake()
    {
        if (playerInventory == null)
            playerInventory = PlayerInventory.Instance;

        if (inventoryCollapse != null)
            inventoryCollapse.StateChanged += OnCollapseStateChanged;

        if (dragGhostImage != null)
        {
            dragGhostImage.enabled = false;
            dragGhostImage.raycastTarget = false;
        }
    }

    private void OnDestroy()
    {
        if (inventoryCollapse != null)
            inventoryCollapse.StateChanged -= OnCollapseStateChanged;
    }

    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.InventoryChanged += Refresh;

        if (refreshOnEnable)
            Refresh();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.InventoryChanged -= Refresh;

        if (detailsUI != null)
            detailsUI.HideImmediate();

        HideDragGhost();
    }

    private void OnCollapseStateChanged(bool isOpen)
    {
        if (!isOpen && detailsUI != null)
            detailsUI.HideImmediate();

        //if (!isOpen)
        //    HideDragGhost();
    }

    public void Refresh()
    {
        if (playerInventory == null || itemSlotPrefab == null || contentRoot == null)
            return;

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        var items = playerInventory.GetAllItems();
        for (int i = 0; i < items.Count; i++)
        {
            InventoryItem slot = Instantiate(itemSlotPrefab, contentRoot);
            slot.Bind(items[i]);

            slot.Clicked += OnItemClicked;

            slot.DragStarted += OnDragStarted;
            slot.Dragging += OnDragging;
            slot.DragEnded += OnDragEnded;
        }
    }

    private void OnItemClicked(UniqueItem_SO item)
    {
        if (detailsUI != null)
            detailsUI.Show(item);
    }

    private void OnDragStarted(UniqueItem_SO item, UnityEngine.EventSystems.PointerEventData e)
    {
        if (!enableDragToWorldZones) return;

        currentlyDraggedItem = item;

        if (dragGhostImage != null)
        {
            dragGhostImage.sprite = item.icon;
            dragGhostImage.enabled = true;
            UpdateDragGhostPosition(e);
        }

        UpdateZoneHover(e.position);
    }

    private void OnDragging(UnityEngine.EventSystems.PointerEventData e)
    {
        if (!enableDragToWorldZones) return;

        UpdateDragGhostPosition(e);
        UpdateZoneHover(e.position);
    }

    private void OnDragEnded(UniqueItem_SO item, UnityEngine.EventSystems.PointerEventData e)
    {
        if (!enableDragToWorldZones) return;

        ClearZoneHover();

        currentlyDraggedItem = null;

        HideDragGhost();

        if (ItemCombinationUI.TryDrop(item, e))
            return;

        DragItemZone.TryDrop(item, e.position);
    }

    private void UpdateZoneHover(Vector2 screenPos)
    {
        var zone = DragItemZone.GetZoneUnderPointer(screenPos);

        if (zone == currentHoverZone)
        {
            if (currentHoverZone != null)
                currentHoverZone.SetHover(true, currentlyDraggedItem);
            return;
        }

        if (currentHoverZone != null)
            currentHoverZone.SetHover(false, currentlyDraggedItem);

        currentHoverZone = zone;

        if (currentHoverZone != null)
            currentHoverZone.SetHover(true, currentlyDraggedItem);
    }

    private void ClearZoneHover()
    {
        if (currentHoverZone != null)
            currentHoverZone.SetHover(false, currentlyDraggedItem);

        currentHoverZone = null;
    }

    private void UpdateDragGhostPosition(UnityEngine.EventSystems.PointerEventData e)
    {
        if (dragGhostImage == null) return;

        RectTransform rt = dragGhostImage.rectTransform;

        if (rootCanvas == null)
        {
            rt.position = e.position;
            return;
        }

        RectTransform canvasRect = rootCanvas.transform as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, e.position, rootCanvas.worldCamera, out var localPos))
        {
            rt.anchoredPosition = localPos;
        }
    }

    private void HideDragGhost()
    {
        if (dragGhostImage != null)
            dragGhostImage.enabled = false;
    }
}
