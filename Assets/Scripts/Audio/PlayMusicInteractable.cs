using UnityEngine;

public class PlayMusicInteractable : Interactable
{
    public enum InteractionTriggerMode { MANUAL, AUTO }

    public enum PlayMusic { MENU, LVL1, LVL2, LVL3, STOP };

    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("ACTION")]
    [SerializeField] private PlayMusic musicToPlay = PlayMusic.LVL1;

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

        OnExitRange();

        switch (musicToPlay)
        {
            case PlayMusic.MENU:
                AudioManager.Instance.PlayMenuMusic();
                break;

            case PlayMusic.LVL1:
                AudioManager.Instance.PlayLvl1Music();
                break;

            case PlayMusic.LVL2:
                AudioManager.Instance.PlayLvl2Music();
                break;

            case PlayMusic.LVL3:
                AudioManager.Instance.PlayLvl3Music();
                break;

            case PlayMusic.STOP:
                AudioManager.Instance.StopMusicImmediate();
                break;
        }

        if (!string.IsNullOrEmpty(levelTriggerId) && LevelManager.Instance != null)
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);

        executed = true;
    }
}
