using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class AutoScrollCredits : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("Scroll Settings")]
    [SerializeField] private float scrollDuration = 30f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = false;
    [SerializeField] private float startDelay = 3f;
    [SerializeField] private float endDelay = 3f;

    [Header("Optional")]
    [SerializeField] private bool stopWhenUserScrolls = true;

    private bool isPlaying;
    private Coroutine scrollRoutine;

    private void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    private void OnEnable()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null) return;

        StartCoroutine(InitAndMaybeStart());
    }

    private IEnumerator InitAndMaybeStart()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 1f;

        if (playOnStart)
            Play();
    }

    public void Play()
    {
        if (scrollRect == null) return;

        if (scrollRoutine != null)
            StopCoroutine(scrollRoutine);

        scrollRoutine = StartCoroutine(ScrollCredits());
    }

    public void Stop()
    {
        isPlaying = false;
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }

        if (scrollRect != null)
            scrollRect.StopMovement();
    }

    public void Restart()
    {
        Stop();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        scrollRect.verticalNormalizedPosition = 1f;
        Play();
    }

    private IEnumerator ScrollCredits()
    {
        isPlaying = true;

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        do
        {
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;

            float elapsed = 0f;

            while (elapsed < scrollDuration && isPlaying)
            {
                if (stopWhenUserScrolls && Input.mouseScrollDelta.y != 0f)
                {
                    Stop();
                    yield break;
                }

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / scrollDuration);

                scrollRect.verticalNormalizedPosition = 1f - t;

                yield return null;
            }

            scrollRect.verticalNormalizedPosition = 0f;

            if (loop && endDelay > 0f)
                yield return new WaitForSeconds(endDelay);

        } while (loop && isPlaying);

        isPlaying = false;
        scrollRoutine = null;
    }
}
