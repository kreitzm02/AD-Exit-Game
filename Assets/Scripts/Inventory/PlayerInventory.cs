using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    public event Action InventoryChanged;

    private HashSet<string> collectedItemIds = new HashSet<string>();
    private List<UniqueItem_SO> collectedItems = new List<UniqueItem_SO>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool HasItem(string itemId)
    {
        return collectedItemIds.Contains(itemId);
    }

    public bool AddItem(UniqueItem_SO item, bool showNotification)
    {
        if (item == null || collectedItemIds.Contains(item.itemId))
            return false;

        collectedItemIds.Add(item.itemId);
        collectedItems.Add(item);

        Debug.Log("[PlayerInventory] Collected Item: " + item.displayName);

        InventoryChanged?.Invoke();

        if (showNotification && ItemNotificationUI.Instance != null)
            ItemNotificationUI.Instance.Show(item);

        return true;
    }

    public bool AddItem(UniqueItem_SO item)
    {
        return AddItem(item, true);
    }

    public IReadOnlyList<UniqueItem_SO> GetAllItems()
    {
        return collectedItems;
    }

    public List<string> GetAllItemIds()
    {
        return new List<string>(collectedItemIds);
    }

    public void ClearInventory(bool notify = true)
    {
        collectedItemIds.Clear();
        collectedItems.Clear();

        if (notify)
            InventoryChanged?.Invoke();

        Debug.Log("[PlayerInventory] Inventory cleared.");
    }

    public void RestoreInventory(IEnumerable<UniqueItem_SO> items, bool showNotifications = false)
    {
        collectedItemIds.Clear();
        collectedItems.Clear();

        if (items != null)
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                AddItem(item, showNotifications);
            }
        }

        InventoryChanged?.Invoke();
        Debug.Log("[PlayerInventory] Inventory restored.");
    }

    public bool RemoveItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        if (!collectedItemIds.Contains(itemId)) return false;

        collectedItemIds.Remove(itemId);

        for (int i = collectedItems.Count - 1; i >= 0; i--)
        {
            if (collectedItems[i] != null && collectedItems[i].itemId == itemId)
            {
                collectedItems.RemoveAt(i);
                break;
            }
        }

        InventoryChanged?.Invoke();
        return true;
    }
}
