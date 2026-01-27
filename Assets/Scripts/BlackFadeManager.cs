using DigitalRuby.Tween;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlackFadeManager : MonoBehaviour
{
    public static BlackFadeManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Image blackImage;

    [Header("Defaults")]
    [SerializeField] private float defaultFadeDuration = 0.8f;
    [SerializeField] private float defaultHoldTime = 0.25f;

    [Header("Behavior")]
    [SerializeField] private bool disableImageWhenTransparent = true;

    private Tween<float> tween;

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

    private void SetAlpha(float a)
    {
        if (blackImage == null) return;
        var c = blackImage.color;
        c.a = Mathf.Clamp01(a);
        blackImage.color = c;
    }
}
