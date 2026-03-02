using FMODUnity;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LockInteractable : Interactable
{
    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("START DIGITS")]
    [Range(0, 9)][SerializeField] private int startA = 0;
    [Range(0, 9)][SerializeField] private int startB = 0;
    [Range(0, 9)][SerializeField] private int startC = 0;

    [Header("CORRECT DIGITS")]
    [Range(0, 9)][SerializeField] private int correctA = 3;
    [Range(0, 9)][SerializeField] private int correctB = 7;
    [Range(0, 9)][SerializeField] private int correctC = 1;

    [Header("TRIGGER ID")]
    [SerializeField] private string levelTriggerId;

    [Header("AUDIO")]
    [SerializeField] private EventReference solvedSFX;

    [Header("SETTINGS")]
    [SerializeField] private bool isDummyLock;

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
        if (triggerMode != InteractionTriggerMode.MANUAL)
            return;

        TryOpen();
    }

    private void TryOpen()
    {
        if (solved || isOpen || !isPlayerInRange)
            return;

        isOpen = true;
        OnExitRange();

        LockUI.Instance.Open(this, startA, startB, startC, correctA, correctB, correctC, isDummyLock);
    }

    public void CloseFromUI()
    {
        isOpen = false;
    }

    public void NotifySolved()
    {
        if (solved) return;
        solved = true;

        AudioManager.Instance.PlaySFX(solvedSFX);

        if (!string.IsNullOrEmpty(levelTriggerId) && LevelManager.Instance != null)
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);
    }
}
