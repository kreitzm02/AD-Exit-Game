using UnityEngine;

public class ContainerInteractable : Interactable
{
    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("SLOTS (6X)")]
    [SerializeField] private ContainerSlot[] slots = new ContainerSlot[6];

    [Header("TRIGGER ID")]
    [SerializeField] private string levelTriggerId;

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

        isOpen = true;
        OnExitRange();

        ContainerUI.Instance.Open(this);
    }

    public void CloseFromUI()
    {
        isOpen = false;
    }
}
