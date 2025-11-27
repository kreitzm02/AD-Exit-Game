using UnityEngine;

public class DoorInteractable : Interactable
{
    [Header("ROOM TARGET")]
    [SerializeField] private string targetRoomId;
    [SerializeField] private int targetEntryPoint;

    public override void Interact()
    {
        if (!isPlayerInRange) return;

        OnExitRange();

        RoomManager.Instance.ChangeRoom(targetRoomId, targetEntryPoint);
    }
}
