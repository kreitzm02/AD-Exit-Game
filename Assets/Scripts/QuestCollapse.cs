using UnityEngine;
using DigitalRuby.Tween;
using TMPro;

public class QuestCollapse : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform questBar;
    [SerializeField] private TextMeshProUGUI questText;

    [Header("Positions")]
    [SerializeField] private Vector2 hiddenPosition;  
    [SerializeField] private Vector2 baseActivePosition; 

    [Header("Dynamic Width")]
    [SerializeField] private float padding = 40f;          
    [SerializeField] private float maxWidth = 900f;

    [Header("Timing")]
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float autoHideDelay = 4f;

    private bool isMoving;
    private Coroutine autoHideRoutine;

    private void Awake()
    {
        questBar.anchoredPosition = hiddenPosition;
    }

    public void ShowQuestBar()
    {
        if (isMoving)
            return;

        Vector2 targetPos = CalculateDynamicActivePosition();
        MoveTo(targetPos);

        if (autoHideRoutine != null)
            StopCoroutine(autoHideRoutine);

        autoHideRoutine = StartCoroutine(AutoHideRoutine());
    }

    private Vector2 CalculateDynamicActivePosition()
    {
        questText.ForceMeshUpdate();

        float textWidth = questText.preferredWidth + padding;
        textWidth = Mathf.Min(textWidth, maxWidth);

        return new Vector2(
            baseActivePosition.x + textWidth,
            baseActivePosition.y
        );
    }

    private void MoveTo(Vector2 target)
    {
        isMoving = true;
        Vector2 start = questBar.anchoredPosition;

        TweenFactory.Tween("QuestBarTween", start, target, moveDuration, TweenScaleFunctions.CubicEaseInOut,
            (ITween<Vector2> tween) =>
            {
                questBar.anchoredPosition = tween.CurrentValue;
            },
            (ITween<Vector2> tween) =>
            {
                questBar.anchoredPosition = target;
                isMoving = false;
            }
        );
    }

    private System.Collections.IEnumerator AutoHideRoutine()
    {
        yield return new WaitForSecondsRealtime(autoHideDelay);
        MoveTo(hiddenPosition);
    }

    public void SetQuestText(string text)
    {
        questText.text = text;
    }
}
