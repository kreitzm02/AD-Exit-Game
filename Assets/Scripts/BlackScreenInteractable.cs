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
    [Tooltip("Optionaler Delay vor der Aktion (Sekunden).")]
    [SerializeField] private float startDelay = 0f;

    [Tooltip("Wenn true: sofort (ohne Fade). Wenn false: Fade verwenden.")]
    [SerializeField] private bool instant = false;

    [Tooltip("Fade-Dauer in Sekunden (nur wenn instant=false).")]
    [SerializeField] private float fadeDuration = 0.8f;

    [Header("LEVEL TRIGGER (OPTIONAL)")]
    [Tooltip("Wenn gesetzt: LevelManager bekommt den Trigger nach Abschluss (instant: sofort nach Set; fade: nach fadeDuration).")]
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

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        bool turnOn = (action == BlackScreenAction.BLACK_ON);

        if (instant)
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

        if (!string.IsNullOrEmpty(levelTriggerId) && LevelManager.Instance != null)
        {
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);
        }

        isRunning = false;
    }
}
