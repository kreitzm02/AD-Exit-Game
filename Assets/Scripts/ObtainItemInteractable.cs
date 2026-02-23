using UnityEngine;

public class ObtainItemInteractable : Interactable
{
    public enum InteractionTriggerMode { MANUAL, AUTO }
    public enum ToggleAction { ADDITEM, REMOVEITEM }

    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("TARGET")]
    [SerializeField] private UniqueItem_SO item;
    [Header("ACTION")]
    [SerializeField] private ToggleAction action = ToggleAction.ADDITEM;

    [Header("LEVEL TRIGGER (OPTIONAL)")]
    [SerializeField] private string levelTriggerId;

    public override void OnEnterRange()
    {
        base.OnEnterRange();

        if (triggerMode == InteractionTriggerMode.AUTO)
            TryExecute();
    }

    public override void Interact()
    {
        if (triggerMode != InteractionTriggerMode.MANUAL)
            return;

        TryExecute();
    }

    private void TryExecute()
    {
        if (!isPlayerInRange || item == null)
            return;

        OnExitRange();

        switch (action)
        {
            case ToggleAction.ADDITEM:
                PlayerInventory.Instance.AddItem(item);
                break;

            case ToggleAction.REMOVEITEM:
                PlayerInventory.Instance.RemoveItem(item.itemId);
                break;
        }

        if (!string.IsNullOrEmpty(levelTriggerId) && LevelManager.Instance != null)
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);
    }
}
