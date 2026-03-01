using UnityEngine;

public class TraceInteractable : Interactable
{
    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("MINIGAME")]
    [SerializeField] private TraceMinigame minigame;
    [SerializeField] private Transform focusTarget;
    [SerializeField] private Vector2 focusOffset;
    [SerializeField] private float zoom = 3.5f;
    [SerializeField] private float cameraDuration = 0.4f;
    [SerializeField] private bool smooth = true;

    [Header("TRIGGER ID (optional)")]
    [SerializeField] private string levelTriggerId;

    private bool isOpen;
    private bool solved;

    public override void OnEnterRange()
    {
        base.OnEnterRange();
        if (triggerMode == InteractionTriggerMode.AUTO)
            TryOpen();
    }

    public override void Interact()
    {
        if (triggerMode != InteractionTriggerMode.MANUAL) return;
        TryOpen();
    }

    private void TryOpen()
    {
        if (solved || isOpen || !isPlayerInRange || !minigame) return;

        isOpen = true;
        OnExitRange();

        var cam = Camera.main.GetComponent<PlayerCamera>();
        cam.FocusOn(focusTarget ? focusTarget : minigame.transform, focusOffset, smooth, cameraDuration);
        cam.ZoomTo(zoom, smooth, cameraDuration);
        cam.SetCameraBoundsActive(false);

        minigame.Open(this);
    }

    public void CloseFromMinigame(bool wasSolved)
    {
        isOpen = false;
        solved = solved || wasSolved;

        var cam = Camera.main.GetComponent<PlayerCamera>();
        cam.ReturnToPlayerAndBounds(smooth, cameraDuration);

        if (solved && !string.IsNullOrEmpty(levelTriggerId) && LevelManager.Instance != null)
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);
    }
}
