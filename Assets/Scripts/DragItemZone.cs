using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class DragItemZone : MonoBehaviour
{
    [Header("Item Filter (optional)")]
    [SerializeField] private string requiredItemId;

    [Header("Behaviour")]
    [SerializeField] private bool consumeItemOnDrop = true;

    [Header("Level Trigger (optional)")]
    [SerializeField] private string levelTriggerId;

    [Header("FMOD (optional)")]
    [SerializeField] private EventReference dropSFX;

    [Header("Highlight (optional)")]
    [SerializeField] private PolygonHighlight highlight;
    [SerializeField] private bool highlightOnlyIfAcceptsItem = true;

    private PolygonCollider2D poly;

    private void Awake()
    {
        poly = GetComponent<PolygonCollider2D>();

        if (highlight != null)
        {
            highlight.SetVisible(false);
        }
    }

    public bool Accepts(UniqueItem_SO item)
    {
        if (item == null) return false;
        if (string.IsNullOrEmpty(requiredItemId)) return true;
        return item.itemId == requiredItemId;
    }

    public void SetHover(bool hovering, UniqueItem_SO draggedItem)
    {
        if (highlight == null) return;

        if (!hovering)
        {
            highlight.SetVisible(false);
            return;
        }

        if (highlightOnlyIfAcceptsItem && !Accepts(draggedItem))
        {
            highlight.SetVisible(false);
            return;
        }

        highlight.SetVisible(true);
    }

    public bool TryHandleDrop(UniqueItem_SO item)
    {
        if (!Accepts(item)) return false;

        if (!string.IsNullOrEmpty(levelTriggerId) && LevelManager.Instance != null)
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);

        if (!dropSFX.IsNull)
            RuntimeManager.PlayOneShot(dropSFX, transform.position);

        if (consumeItemOnDrop && PlayerInventory.Instance != null)
            PlayerInventory.Instance.RemoveItem(item.itemId);

        // LevelManager.Instance.SaveCurrentGame();

        return true;
    }

    public static DragItemZone GetZoneUnderPointer(Vector2 screenPosition)
    {
        if (Camera.main == null) return null;

        Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
        Vector2 world2D = new Vector2(world.x, world.y);

        var hits = Physics2D.OverlapPointAll(world2D);
        if (hits == null || hits.Length == 0) return null;

        for (int i = 0; i < hits.Length; i++)
        {
            var zone = hits[i].GetComponentInParent<DragItemZone>();
            if (zone != null) return zone;
        }

        return null;
    }

    public static bool TryDrop(UniqueItem_SO item, Vector2 screenPosition)
    {
        var zone = GetZoneUnderPointer(screenPosition);
        if (zone == null) return false;

        return zone.TryHandleDrop(item);
    }
}
