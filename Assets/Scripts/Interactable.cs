using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] protected Transform uiAnchor;

    protected bool isPlayerInRange;

    public virtual void OnEnterRange()
    {
        isPlayerInRange = true;
        InteractionUI.Instance.Show(uiAnchor);
    }

    public virtual void OnExitRange()
    {
        isPlayerInRange = false;
        InteractionUI.Instance.Hide();
    }

    public abstract void Interact();
}

public enum InteractionTriggerMode
{
    MANUAL,     
    AUTO,   
}
