using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] protected Transform uiAnchor;

    [Header("Highlight")]
    [SerializeField] private PolygonHighlight highlight;

    protected bool isPlayerInRange;

    public Transform UiAnchor => uiAnchor;

    public virtual void OnEnterRange()
    {
        isPlayerInRange = true;
        if (highlight) highlight.SetVisible(true);

        LevelManager.Instance.SaveCurrentGame();
    }

    public virtual void OnExitRange()
    {
        isPlayerInRange = false;
        if (highlight) highlight.SetVisible(false);
    }

    public abstract void Interact();
}

public enum InteractionTriggerMode
{
    MANUAL,
    AUTO,
}
