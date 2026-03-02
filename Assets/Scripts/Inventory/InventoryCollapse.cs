using DigitalRuby.Tween;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryCollapse : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private RectTransform uiElement;

    [Header("Positions")]
    [SerializeField] private Vector2 positionA; 
    [SerializeField] private Vector2 positionB; 

    [Header("Settings")]
    [SerializeField] private float moveDuration = 0.3f;

    public bool IsOpen { get; private set; } = false;
    public event System.Action<bool> StateChanged;

    private bool isMoving;

    private bool forceClosed = false;

    private void Start()
    {
        GameEvents.OnCutsceneRunning += HandleCutsceneRunning;
        GameEvents.OnItemUsed += HandleItemUsed;
    }

    public void Toggle()
    {
        if (isMoving || forceClosed)
            return;

        var current = uiElement.anchoredPosition;
        float distToA = Vector2.Distance(current, positionA);
        float distToB = Vector2.Distance(current, positionB);

        Vector2 target = (distToA < distToB) ? positionB : positionA;

        bool nextOpen = (target == positionA);
        IsOpen = nextOpen;
        StateChanged?.Invoke(IsOpen);

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

    private void HandleCutsceneRunning(bool isRunning)
    {
        if (IsOpen && isRunning) Toggle();

        forceClosed = isRunning;
    }

    private void HandleItemUsed()
    {
        if (IsOpen) Toggle();
    }
}
