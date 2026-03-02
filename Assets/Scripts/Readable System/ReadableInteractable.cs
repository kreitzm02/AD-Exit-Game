using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadableInteractable : Interactable
{
    public enum PageMode
    {
        ManualPages,
        AutoSplit
    }

    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("PAGE MODE")]
    [SerializeField] private PageMode pageMode = PageMode.ManualPages;

    [Header("PAGES (Manual Mode)")]
    [SerializeField] private ReadablePage[] pages;

    [Header("AUTO SPLIT (Auto Mode)")]
    [TextArea(10, 25)]
    [SerializeField] private string fullText;

    [SerializeField, Min(1)]
    private int charLimitPerPage = 480;

    [SerializeField] private bool preserveWords = true;

    [Header("TEXT STYLE")]
    [SerializeField, Min(1f)]
    private float fontSize = 22.5f;

    [Header("TRIGGER ID")]
    [SerializeField] private string levelTriggerId;

    [Header("CLOSE BUTTON")]
    [SerializeField] private Button closeButton;

    private bool isReading;

    private ReadablePage[] autoPagesCache;

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
        if (!isPlayerInRange || isReading)
            return;

        ReadablePage[] pagesToOpen = GetPagesToOpen();
        if (pagesToOpen == null || pagesToOpen.Length == 0)
        {
            Debug.LogWarning("[ReadableInteractable] No pages to open (check mode and assigned text).");
            return;
        }

        isReading = true;
        OnExitRange();

        ReadableUI.Instance.Open(pagesToOpen, fontSize);

        closeButton = GameObject.FindWithTag("ReadableCloseButton")?.GetComponent<Button>();
        if (closeButton)
        {
            closeButton.onClick.AddListener(CloseFromUI);
        }
        else
        {
            Debug.LogWarning("[ReadableInteractable] Close button with tag 'ReadableCloseButton' not found.");
        }
    }

    private ReadablePage[] GetPagesToOpen()
    {
        if (pageMode == PageMode.ManualPages)
        {
            return pages;
        }

        if (autoPagesCache == null)
            autoPagesCache = BuildAutoPages(fullText, charLimitPerPage, preserveWords);

        return autoPagesCache;
    }

    private static ReadablePage[] BuildAutoPages(string text, int limit, bool preserveWords)
    {
        if (string.IsNullOrEmpty(text) || limit <= 0)
            return new ReadablePage[0];

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        var result = new List<ReadablePage>();
        int i = 0;

        while (i < text.Length)
        {
            int remaining = text.Length - i;
            int take = Mathf.Min(limit, remaining);
            int end = i + take;

            if (end < text.Length && preserveWords)
            {
                int lastWhitespace = -1;
                for (int j = end - 1; j > i; j--)
                {
                    if (char.IsWhiteSpace(text[j]))
                    {
                        lastWhitespace = j;
                        break;
                    }
                }

                if (lastWhitespace > i)
                    end = lastWhitespace + 1;
            }

            string pageText = text.Substring(i, end - i).TrimEnd();

            result.Add(new ReadablePage { text = pageText });

            i = end;
            while (i < text.Length && char.IsWhiteSpace(text[i]))
                i++;
        }

        return result.ToArray();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        autoPagesCache = null;
        if (pageMode == PageMode.AutoSplit)
        {
            autoPagesCache = BuildAutoPages(fullText, charLimitPerPage, preserveWords);
        }
    }
#endif

    public void CloseFromUI()
    {
        isReading = false;

        if (closeButton)
            closeButton.onClick.RemoveListener(CloseFromUI);

        if (!string.IsNullOrEmpty(levelTriggerId))
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);
    }

    public void SetFontSize(float newSize)
    {
        fontSize = Mathf.Max(1f, newSize);
    }

    public void SetFullText(string newText)
    {
        fullText = newText;
        autoPagesCache = null;
    }
}
