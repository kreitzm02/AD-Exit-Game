using UnityEngine;

public class DoorInteractable : Interactable
{
    [Header("ROOM TARGET")]
    [SerializeField] private string targetRoomId;
    [SerializeField] private int targetEntryPoint;

    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("TRANSITION")]
    [SerializeField] private bool useRoomManagerFade = true;

    [Header("LEVEL TRIGGER (OPTIONAL)")]
    [SerializeField] private string levelTriggerId;

    public override void Interact()
    {
        if (triggerMode != InteractionTriggerMode.MANUAL)
            return;

        TryUseDoor();
    }

    public override void OnEnterRange()
    {
        base.OnEnterRange();

        if (triggerMode == InteractionTriggerMode.AUTO)
        {
            TryUseDoor();
        }
    }

    private void TryUseDoor()
    {
        if (!isPlayerInRange) return;

        OnExitRange();

        if (useRoomManagerFade)
            RoomManager.Instance.ChangeRoom(targetRoomId, targetEntryPoint);
        else
            RoomManager.Instance.ChangeRoomNoFade(targetRoomId, targetEntryPoint);

        if (!string.IsNullOrEmpty(levelTriggerId) && LevelManager.Instance != null)
        {
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);
        }
    }
}
