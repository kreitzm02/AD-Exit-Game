using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemNotificationUI : MonoBehaviour
{
    public static ItemNotificationUI Instance;

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float displayDuration = 1.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    private Coroutine routine;

    private void Awake()
    {
        Instance = this;
        SetVisible(false, instant: true);
    }

    public void Show(UniqueItem_SO item)
    {
        if (item == null) return;

        if (iconImage != null)
        {
            iconImage.enabled = item.icon != null;
            iconImage.sprite = item.icon;
        }

        if (nameText != null)
            nameText.text = item.displayName;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        SetVisible(true, instant: false);
        yield return Fade(canvasGroup, canvasGroup.alpha, 1f, fadeInDuration);

        yield return new WaitForSeconds(displayDuration);

        yield return Fade(canvasGroup, canvasGroup.alpha, 0f, fadeOutDuration);
        SetVisible(false, instant: true);
        routine = null;
    }

    private void SetVisible(bool visible, bool instant)
    {
        if (canvasGroup == null) return;

        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        if (instant)
            canvasGroup.alpha = visible ? 1f : 0f;
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        if (duration <= 0f)
        {
            cg.alpha = to;
            yield break;
        }

        float t = 0f;
        cg.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        cg.alpha = to;
    }
}
