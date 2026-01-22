using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventoryItem itemSlotPrefab;
    [SerializeField] private Transform contentRoot;

    [Header("Optional")]
    [SerializeField] private bool refreshOnEnable = true;

    private void Awake()
    {
        if (playerInventory == null)
            playerInventory = PlayerInventory.Instance;
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
    }

    public void Refresh()
    {
        if (playerInventory == null || itemSlotPrefab == null || contentRoot == null)
            return;

        // Clear
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        // Rebuild
        var items = playerInventory.GetAllItems();
        for (int i = 0; i < items.Count; i++)
        {
            InventoryItem slot = Instantiate(itemSlotPrefab, contentRoot);
            slot.Bind(items[i]);
        }
    }
}
