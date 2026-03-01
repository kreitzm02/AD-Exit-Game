using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemCombinationUI : MonoBehaviour, IDropHandler
{
    [Header("Recipes")]
    [SerializeField] private CombinationRecipeDb_SO recipeDatabase;

    [Header("UI")]
    [SerializeField] private Image slotAIcon;
    [SerializeField] private Image slotBIcon;
    [SerializeField] private Image resultPreviewIcon;
    [SerializeField] private float combineDelay = 1.0f;

    [Header("Settings")]
    [SerializeField] private bool clearOnInvalidSecondDrop = true;

    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Idle Reset")]
    [SerializeField, Min(0.1f)] private float idleResetSeconds = 10f;

    private bool isDragging;

    public bool HasPendingItem => slotA != null || slotB != null || combineRoutine != null;

    private UniqueItem_SO slotA;
    private UniqueItem_SO slotB;
    private Coroutine combineRoutine;

    private Coroutine idleRoutine;

    private void OnEnable()
    {
        RefreshIcons();
        RefreshResultPreview();
        RefreshVisibility();
        RestartIdleTimerIfNeeded();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null) return;

        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var invItem = dragged.GetComponentInParent<InventoryItem>();
        if (invItem == null) return;

        TryHandleDrop(invItem.BoundItem);
    }

    public void SetDragging(bool dragging)
    {
        isDragging = dragging;
        RefreshVisibility();

        if (isDragging)
        {
            StopIdleTimer();
        }
        else
        {
            RestartIdleTimerIfNeeded();
        }
    }

    public bool TryHandleDrop(UniqueItem_SO item)
    {
        if (item == null) return false;

        if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasItem(item.itemId))
            return false;

        if (combineRoutine != null)
            return false;

        TouchActivity();

        if (slotA == null)
        {
            slotA = item;
            RefreshIcons();
            RefreshResultPreview();
            RefreshVisibility();
            RestartIdleTimerIfNeeded();
            return true;
        }

        if (slotA != null && slotA.itemId == item.itemId)
            return false;

        if (slotB == null)
        {
            slotB = item;
            RefreshIcons();
            RefreshResultPreview();
            RefreshVisibility();

            if (TryGetResult(slotA, slotB, out var result))
            {
                StopIdleTimer();
                combineRoutine = StartCoroutine(CombineRoutine(slotA, slotB, result));
            }
            else
            {
                if (clearOnInvalidSecondDrop)
                {
                    Clear();
                }
                else
                {
                    RestartIdleTimerIfNeeded();
                }
            }

            return true;
        }

        slotB = item;
        RefreshIcons();
        RefreshResultPreview();
        RefreshVisibility();

        if (TryGetResult(slotA, slotB, out var result2))
        {
            StopIdleTimer();
            combineRoutine = StartCoroutine(CombineRoutine(slotA, slotB, result2));
        }
        else if (clearOnInvalidSecondDrop)
        {
            Clear();
        }
        else
        {
            RestartIdleTimerIfNeeded();
        }

        return true;
    }

    public void Clear()
    {
        StopIdleTimer();

        if (combineRoutine != null)
        {
            StopCoroutine(combineRoutine);
            combineRoutine = null;
        }

        slotA = null;
        slotB = null;

        RefreshIcons();
        RefreshResultPreview();
        RefreshVisibility();
    }

    private IEnumerator CombineRoutine(UniqueItem_SO a, UniqueItem_SO b, UniqueItem_SO result)
    {
        yield return new WaitForSecondsRealtime(combineDelay);

        if (PlayerInventory.Instance == null)
        {
            Clear();
            yield break;
        }

        bool removedA = PlayerInventory.Instance.RemoveItem(a.itemId);
        bool removedB = PlayerInventory.Instance.RemoveItem(b.itemId);

        if (!removedA || !removedB)
        {
            if (removedA && !PlayerInventory.Instance.HasItem(a.itemId))
                PlayerInventory.Instance.AddItem(a, false);
            if (removedB && !PlayerInventory.Instance.HasItem(b.itemId))
                PlayerInventory.Instance.AddItem(b, false);

            Clear();
            yield break;
        }

        PlayerInventory.Instance.AddItem(result, true);

        Clear();
    }

    private bool TryGetResult(UniqueItem_SO a, UniqueItem_SO b, out UniqueItem_SO result)
    {
        result = null;
        if (recipeDatabase == null || a == null || b == null) return false;
        return recipeDatabase.TryGetResult(a.itemId, b.itemId, out result);
    }

    private void RefreshIcons()
    {
        if (slotAIcon != null)
        {
            slotAIcon.enabled = slotA != null && slotA.icon != null;
            slotAIcon.sprite = slotA != null ? slotA.icon : null;
        }

        if (slotBIcon != null)
        {
            slotBIcon.enabled = slotB != null && slotB.icon != null;
            slotBIcon.sprite = slotB != null ? slotB.icon : null;
        }
    }

    private void RefreshResultPreview()
    {
        if (resultPreviewIcon == null)
            return;

        if (slotA == null || slotB == null)
        {
            resultPreviewIcon.enabled = false;
            resultPreviewIcon.sprite = null;
            return;
        }

        if (TryGetResult(slotA, slotB, out var result) && result != null && result.icon != null)
        {
            resultPreviewIcon.enabled = true;
            resultPreviewIcon.sprite = result.icon;
        }
        else
        {
            resultPreviewIcon.enabled = false;
            resultPreviewIcon.sprite = null;
        }
    }

    public static bool TryDrop(UniqueItem_SO item, PointerEventData e)
    {
        if (e == null) return false;

        var go = e.pointerEnter;
        if (go == null) return false;

        var zone = go.GetComponentInParent<ItemCombinationUI>();
        if (zone == null) return false;

        return zone.TryHandleDrop(item);
    }

    private void RefreshVisibility()
    {
        bool shouldShow = isDragging || HasPendingItem;
        SetVisible(shouldShow);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }

    private void TouchActivity()
    {
        StopIdleTimer();
        RestartIdleTimerIfNeeded();
    }

    private void RestartIdleTimerIfNeeded()
    {
        if (isDragging) return;
        if (combineRoutine != null) return;
        if (slotA == null && slotB == null) return;

        StopIdleTimer();
        idleRoutine = StartCoroutine(IdleResetRoutine());
    }

    private void StopIdleTimer()
    {
        if (idleRoutine != null)
        {
            StopCoroutine(idleRoutine);
            idleRoutine = null;
        }
    }

    private IEnumerator IdleResetRoutine()
    {
        yield return new WaitForSecondsRealtime(idleResetSeconds);

        if (isDragging) { idleRoutine = null; yield break; }
        if (combineRoutine != null) { idleRoutine = null; yield break; }

        if (slotA != null || slotB != null)
            Clear();

        idleRoutine = null;
    }
}
