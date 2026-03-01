using System.Collections;
using UnityEngine;

public class BlackScreenInteractable : Interactable
{
    public enum BlackScreenAction
    {
        BLACK_ON,
        BLACK_OFF
    }

    [Header("ACTION")]
    [SerializeField] private BlackScreenAction action = BlackScreenAction.BLACK_ON;

    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("TIMING")]
    [SerializeField] private float startDelay = 0f;

    [SerializeField] private bool instantNoFade = false;

    [SerializeField] private float fadeDuration = 0.8f;

    [Header("OPTIONAL TEXT SEQUENCE")]
    [SerializeField] private bool playTextSequence = false;

    [SerializeField] private BlackScreenTextStep[] textSteps;

    [Header("LEVEL TRIGGER (OPTIONAL)")]
    [SerializeField] private string levelTriggerId;

    private bool isRunning;

    public override void OnEnterRange()
    {
        base.OnEnterRange();

        if (triggerMode == InteractionTriggerMode.AUTO)
        {
            TryExecute();
        }
    }

    public override void Interact()
    {
        if (triggerMode != InteractionTriggerMode.MANUAL)
            return;

        TryExecute();
    }

    private void TryExecute()
    {
        if (!isPlayerInRange || isRunning)
            return;

        if (BlackFadeManager.Instance == null)
        {
            Debug.LogError("[BlackScreenInteractable] Missing BlackFadeController.Instance in scene.");
            return;
        }

        OnExitRange();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        isRunning = true;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        player.LockInput(true);

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        bool turnOn = true;

        if (action == BlackScreenAction.BLACK_ON)
        {
            turnOn = true;
        }
        else turnOn = false;

        if (instantNoFade)
        {
            BlackFadeManager.Instance.SetBlackInstant(turnOn);
        }
        else
        {
            if (turnOn)
                BlackFadeManager.Instance.FadeToBlack(duration: fadeDuration);
            else
                BlackFadeManager.Instance.FadeFromBlack(duration: fadeDuration);

            if (fadeDuration > 0f)
                yield return new WaitForSeconds(fadeDuration);
        }

        if (playTextSequence && textSteps != null && textSteps.Length > 0)
        {
            yield return BlackFadeManager.Instance.PlayOverlayTextSequence(textSteps);
        }
        else
        {
            BlackFadeManager.Instance.ClearOverlayTextInstant();
        }

        if (!string.IsNullOrEmpty(levelTriggerId) && LevelManager.Instance != null)
        {
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);
        }

        player.LockInput(false);

        isRunning = false;
    }
}
