using TMPro;
using UnityEngine;

public class SubtitleUI : MonoBehaviour
{
    public static SubtitleUI Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI subtitleText;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string text)
    {
        subtitleText.text = text;
        canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        subtitleText.text = "";
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
