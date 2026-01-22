using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCollapse : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private RectTransform uiElement;

    [Header("Positions")]
    [SerializeField] private Vector2 positionA;
    [SerializeField] private Vector2 positionB;

    [Header("Settings")]
    [SerializeField] private float moveDuration = 0.3f;

    private bool isMoving;

    private void Awake()
    {
    }

    public void Toggle()
    {
        if (isMoving)
            return;

        var current = uiElement.anchoredPosition;
        float distToA = Vector2.Distance(current, positionA);
        float distToB = Vector2.Distance(current, positionB);

        Vector2 target = (distToA < distToB) ? positionB : positionA;
        MoveTo(target);
    }

    private void MoveTo(Vector2 target)
    {
        isMoving = true;

        Vector2 start = uiElement.anchoredPosition;

        TweenFactory.Tween("UIToggleSlide", start, target, moveDuration, TweenScaleFunctions.CubicEaseInOut,
            (ITween<Vector2> tween) =>               
            {
                uiElement.anchoredPosition = tween.CurrentValue;
            },
            (ITween<Vector2> tween) =>               
            {
                uiElement.anchoredPosition = target;
                isMoving = false;
            }
        );
    }
}
