using UnityEngine;

public class SpriteToggleInteractable : Interactable
{
    public enum InteractionTriggerMode { MANUAL, AUTO }
    public enum ToggleAction { ENABLE, DISABLE, TOGGLE }

    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("TARGET")]
    [SerializeField] private GameObject target;

    [Header("ACTION")]
    [SerializeField] private ToggleAction action = ToggleAction.TOGGLE;

    [Header("LEVEL TRIGGER (OPTIONAL)")]
    [SerializeField] private string levelTriggerId;

    private bool executed;

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
        if (!isPlayerInRange || executed)
            return;

        if (target == null)
        {
            Debug.LogError("[SpriteToggleInteractable] target is not assigned.");
            return;
        }

        OnExitRange();

        switch (action)
        {
            case ToggleAction.ENABLE:
                target.SetActive(true);
                break;

            case ToggleAction.DISABLE:
                target.SetActive(false);
                break;

            case ToggleAction.TOGGLE:
                target.SetActive(!target.activeSelf);
                break;
        }

        if (!string.IsNullOrEmpty(levelTriggerId) && LevelManager.Instance != null)
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);

        executed = true;
    }
}
