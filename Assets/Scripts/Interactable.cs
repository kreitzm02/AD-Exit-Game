using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] protected Transform uiAnchor;

    [Header("Highlight")]
    [SerializeField] private PolygonHighlight highlight;

    protected bool isPlayerInRange;

    public virtual void OnEnterRange()
    {
        isPlayerInRange = true;
        InteractionUI.Instance.Show(uiAnchor);
        if (highlight) highlight.SetVisible(true);
    }

    public virtual void OnExitRange()
    {
        isPlayerInRange = false;
        InteractionUI.Instance.Hide();
        if (highlight) highlight.SetVisible(false);
    }

    public abstract void Interact();
}

public enum InteractionTriggerMode
{
    MANUAL,     
    AUTO,   
}
