using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

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

    public bool AddItem(UniqueItem_SO item)
    {
        if (item == null || collectedItemIds.Contains(item.itemId))
            return false;

        collectedItemIds.Add(item.itemId);
        collectedItems.Add(item);

        Debug.Log("[PlayerInventory] Collected Item: " + item.displayName);
        return true;
    }

    public IReadOnlyList<UniqueItem_SO> GetAllItems()
    {
        return collectedItems;
    }
}
