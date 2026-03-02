using System.Collections;
using UnityEngine;

public class InvDragCombineController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryCollapse inventoryCollapse;
    [SerializeField] private ItemCombinationUI combinationUI;

    [Header("Settings")]
    [SerializeField] private float collapseDelay = 0.12f;

    [SerializeField] private bool reopenAfterDragIfNoPending = true;

    [SerializeField] private float reopenDelay = 0.05f;

    private Coroutine collapseRoutine;
    private Coroutine reopenRoutine;

    private void OnEnable()
    {
        InventoryItem.AnyDragStateChanged += OnAnyDragStateChanged;

        if (combinationUI != null)
            combinationUI.SetDragging(false);
    }

    private void OnDisable()
    {
        InventoryItem.AnyDragStateChanged -= OnAnyDragStateChanged;
    }

    private void OnAnyDragStateChanged(bool dragging)
    {
        if (combinationUI != null)
            combinationUI.SetDragging(dragging);

        if (dragging)
        {
            if (reopenRoutine != null) { StopCoroutine(reopenRoutine); reopenRoutine = null; }

            if (collapseRoutine != null) StopCoroutine(collapseRoutine);
            collapseRoutine = StartCoroutine(CollapseDelayed());
        }
        else
        {
            if (collapseRoutine != null) { StopCoroutine(collapseRoutine); collapseRoutine = null; }

            if (reopenAfterDragIfNoPending)
            {
                if (reopenRoutine != null) StopCoroutine(reopenRoutine);
                reopenRoutine = StartCoroutine(ReopenDelayedIfNoPending());
            }
        }
    }

    private IEnumerator CollapseDelayed()
    {
        yield return new WaitForSecondsRealtime(collapseDelay);

        if (inventoryCollapse != null && inventoryCollapse.IsOpen)
            inventoryCollapse.Toggle();

        collapseRoutine = null;
    }

    private IEnumerator ReopenDelayedIfNoPending()
    {
        yield return new WaitForSecondsRealtime(reopenDelay);

        bool pending = (combinationUI != null && combinationUI.HasPendingItem);
        if (pending && inventoryCollapse != null && !inventoryCollapse.IsOpen)
            inventoryCollapse.Toggle();

        reopenRoutine = null;
    }
}
