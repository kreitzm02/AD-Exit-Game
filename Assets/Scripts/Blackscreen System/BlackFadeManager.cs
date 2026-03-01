using DigitalRuby.Tween;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackFadeManager : MonoBehaviour
{
    public static BlackFadeManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Image blackImage;

    [Header("Optional Text Overlay")]
    [SerializeField] private TMP_Text overlayText;

    [SerializeField] private CanvasGroup overlayTextGroup;

    [Header("Defaults")]
    [SerializeField] private float defaultFadeDuration = 0.8f;
    [SerializeField] private float defaultHoldTime = 0.25f;

    [Header("Behavior")]
    [SerializeField] private bool disableImageWhenTransparent = true;

    private Tween<float> tween;
    private Tween<float> textTween;

    private EventInstance currentAudio;
    private bool hasCurrentAudio;

    public float CurrentAlpha => blackImage != null ? blackImage.color.a : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (blackImage != null)
        {
            SetAlpha(0f);
            if (disableImageWhenTransparent)
                blackImage.gameObject.SetActive(false);
        }

        SetTextAlpha(0f);
    }

    public void SetBlackInstant(bool black)
    {
        EnsureActive();
        StopTween();

        SetAlpha(black ? 1f : 0f);

        if (!black && disableImageWhenTransparent)
            blackImage.gameObject.SetActive(false);
    }

    public void FadeToBlack(Action onComplete = null, float? duration = null)
        => Fade(CurrentAlpha, 1f, onComplete, duration ?? defaultFadeDuration);

    public void FadeFromBlack(Action onComplete = null, float? duration = null)
        => Fade(CurrentAlpha, 0f, onComplete, duration ?? defaultFadeDuration);

    public void FadeTeleportFade(Action teleportAction, Action onComplete = null, float? fadeDuration = null, float? holdTime = null)
    {
        float dur = fadeDuration ?? defaultFadeDuration;
        float hold = holdTime ?? defaultHoldTime;

        Fade(CurrentAlpha, 1f, () =>
        {
            teleportAction?.Invoke();
            StartCoroutine(HoldThenFadeOut(hold, dur, onComplete));
        }, dur);
    }

    public void ClearOverlayTextInstant()
    {
        StopTextTween();
        StopStepAudio(immediate: false);

        if (overlayText != null)
            overlayText.text = string.Empty;

        SetTextAlpha(0f);
    }

    public Coroutine PlayOverlayTextSequence(BlackScreenTextStep[] steps)
    {
        if (steps == null || steps.Length == 0)
            return null;

        return StartCoroutine(PlayOverlayTextSequenceRoutine(steps));
    }

    private IEnumerator HoldThenFadeOut(float holdSeconds, float fadeOutDuration, Action onComplete)
    {
        if (holdSeconds > 0f)
            yield return new WaitForSeconds(holdSeconds);

        Fade(CurrentAlpha, 0f, onComplete, fadeOutDuration);
    }

    private void Fade(float from, float to, Action onComplete, float duration)
    {
        if (blackImage == null) return;

        EnsureActive();
        StopTween();

        tween = gameObject.Tween(
            "BlackFade",
            from,
            to,
            duration,
            TweenScaleFunctions.Linear,
            (t) => SetAlpha(t.CurrentValue),
            (t) =>
            {
                SetAlpha(to);

                if (to <= 0f && disableImageWhenTransparent)
                    blackImage.gameObject.SetActive(false);

                onComplete?.Invoke();
            }
        );
    }

    private IEnumerator PlayOverlayTextSequenceRoutine(BlackScreenTextStep[] steps)
    {
        if (overlayText == null)
        {
            Debug.LogWarning("[BlackFadeManager] overlayText is not assigned. Text steps will be skipped.");
            yield break;
        }

        EnsureActive();
        SetTextAlpha(0f);

        foreach (var step in steps)
        {
            overlayText.text = step.text;

            //StopStepAudio(immediate: false);
            StartStepAudio(step.fmodEventRef);

            if (step.fadeInDuration > 0f)
                yield return FadeTextRoutine(0f, 1f, step.fadeInDuration);
            else
                SetTextAlpha(1f);

            if (step.holdDuration > 0f)
                yield return new WaitForSeconds(step.holdDuration);

            if (step.fadeOutDuration > 0f)
                yield return FadeTextRoutine(1f, 0f, step.fadeOutDuration);
            else
                SetTextAlpha(0f);

            overlayText.text = string.Empty;
        }

        // StopStepAudio(immediate: false);
        SetTextAlpha(0f);
    }

    private IEnumerator FadeTextRoutine(float from, float to, float duration)
    {
        bool done = false;
        FadeText(from, to, duration, () => done = true);
        while (!done)
            yield return null;
    }

    private void FadeText(float from, float to, float duration, Action onComplete)
    {
        StopTextTween();

        textTween = gameObject.Tween(
            "BlackOverlayTextFade",
            from,
            to,
            duration,
            TweenScaleFunctions.Linear,
            t => SetTextAlpha(t.CurrentValue),
            t =>
            {
                SetTextAlpha(to);
                onComplete?.Invoke();
            }
        );
    }

    private void EnsureActive()
    {
        if (blackImage == null) return;
        if (!blackImage.gameObject.activeSelf)
            blackImage.gameObject.SetActive(true);
    }

    private void StopTween()
    {
        tween?.Stop(TweenStopBehavior.DoNotModify);
        tween = null;
    }

    private void StopTextTween()
    {
        textTween?.Stop(TweenStopBehavior.DoNotModify);
        textTween = null;
    }

    private void SetAlpha(float a)
    {
        if (blackImage == null) return;
        var c = blackImage.color;
        c.a = Mathf.Clamp01(a);
        blackImage.color = c;
    }

    private void SetTextAlpha(float a)
    {
        a = Mathf.Clamp01(a);

        if (overlayTextGroup != null)
        {
            overlayTextGroup.alpha = a;
            return;
        }

        if (overlayText != null)
        {
            var c = overlayText.color;
            c.a = a;
            overlayText.color = c;
        }
    }

    private void StartStepAudio(EventReference eventRef)
    {
        if (eventRef.IsNull) return;

        currentAudio = RuntimeManager.CreateInstance(eventRef);

        var pos = Camera.main ? Camera.main.transform.position : Vector3.zero;
        currentAudio.set3DAttributes(RuntimeUtils.To3DAttributes(pos));

        currentAudio.start();
        hasCurrentAudio = true;
    }

    private void StopStepAudio(bool immediate)
    {
        if (!hasCurrentAudio) return;

        currentAudio.stop(immediate ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentAudio.release();
        hasCurrentAudio = false;
    }
}
