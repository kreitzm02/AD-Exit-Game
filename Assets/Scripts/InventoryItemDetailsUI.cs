using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemDetailsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.12f;
    [SerializeField] private float displayDuration = 9999f;
    [SerializeField] private float fadeOutDuration = 0.12f;

    private Coroutine routine;
    private UniqueItem_SO currentItem;

    private void Awake()
    {
        HideImmediate();
    }

    public void Show(UniqueItem_SO item)
    {
        currentItem = item;
        if (item == null) { HideImmediate(); return; }

        if (iconImage != null)
        {
            iconImage.enabled = item.icon != null;
            iconImage.sprite = item.icon;
        }

        if (nameText != null) nameText.text = item.displayName;
        if (descriptionText != null) descriptionText.text = item.description;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    public void Hide()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(HideRoutine());
    }

    public void HideImmediate()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;

        if (canvasGroup == null) return;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        currentItem = null;
    }

    private IEnumerator ShowRoutine()
    {
        SetInteractable(true);
        yield return Fade(canvasGroup, canvasGroup.alpha, 1f, fadeInDuration);

        if (displayDuration < 9990f)
        {
            yield return new WaitForSecondsRealtime(displayDuration);
            yield return Fade(canvasGroup, canvasGroup.alpha, 0f, fadeOutDuration);
            SetInteractable(false);
            currentItem = null;
        }

        routine = null;
    }

    private IEnumerator HideRoutine()
    {
        yield return Fade(canvasGroup, canvasGroup.alpha, 0f, fadeOutDuration);
        SetInteractable(false);
        currentItem = null;
        routine = null;
    }

    private void SetInteractable(bool value)
    {
        if (canvasGroup == null) return;
        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
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
