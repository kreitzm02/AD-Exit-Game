using FMODUnity;
using UnityEngine;

public class ContainerInteractable : Interactable
{
    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("SLOTS (6X)")]
    [SerializeField] private ContainerSlot[] slots = new ContainerSlot[6];

    [Header("TRIGGER ID")]
    [SerializeField] private string levelTriggerId;

    [Header("SETINGS")]
    [SerializeField] private bool isStorageRoom = false;

    [Header("CONTAINER AUDIO")]
    [SerializeField] private EventReference containerSFX;

    [Header("READABLE TEXT STYLE")]
    [SerializeField, Min(1f)] private float readableFontSize = 22.5f;

    public float ReadableFontSize => readableFontSize;

    public ContainerSlot[] Slots => slots;

    private bool isOpen;

    public override void OnEnterRange()
    {
        base.OnEnterRange();

        if (triggerMode == InteractionTriggerMode.AUTO)
            TryOpen();
    }

    public override void Interact()
    {
        if (triggerMode != InteractionTriggerMode.MANUAL)
            return;

        TryOpen();
    }

    private void TryOpen()
    {
        if (isOpen || !isPlayerInRange)
            return;

        Debug.Log("CONTAINER");

        isOpen = true;
        OnExitRange();

        if (!containerSFX.IsNull) RuntimeManager.PlayOneShot(containerSFX, Camera.main.transform.position);

        if (!isStorageRoom) ContainerUI.Instance.Open(this);
        else StorageRoomUI.Instance.Open(this);
    }

    public void CloseFromUI()
    {
        isOpen = false;
    }
}
