using UnityEngine;

public class ReadableInteractable : Interactable
{
    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("PAGES")]
    [SerializeField] private ReadablePage[] pages;

    [Header("TRIGGER ID")]
    [SerializeField] private string levelTriggerId;

    private bool isReading;

    public override void OnEnterRange()
    {
        base.OnEnterRange();

        if (triggerMode == InteractionTriggerMode.AUTO)
        {
            TryOpen();
        }
    }

    public override void Interact()
    {
        if (triggerMode != InteractionTriggerMode.MANUAL)
            return;

        TryOpen();
    }

    private void TryOpen()
    {
        if (!isPlayerInRange || isReading)
            return;

        if (pages == null || pages.Length == 0)
        {
            Debug.LogWarning("[ReadableInteractable] No pages assigned.");
            return;
        }

        isReading = true;
        OnExitRange();

        ReadableUI.Instance.Open(pages);
    }

    public void CloseFromUI()
    {
        isReading = false;

        if (!string.IsNullOrEmpty(levelTriggerId))
        {
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);
        }
    }
}
