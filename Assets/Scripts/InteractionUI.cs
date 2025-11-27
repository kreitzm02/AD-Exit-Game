using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [SerializeField] private RectTransform rectTransform;

    private Transform currentAnchor;

    private void Awake()
    {
        Instance = this;

        if (!rectTransform)
            rectTransform = GetComponent<RectTransform>();

        Hide();
    }

    private void LateUpdate()
    {
        if (!currentAnchor) return;

        rectTransform.position = currentAnchor.position;
    }

    public void Show(Transform anchor)
    {
        currentAnchor = anchor;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        currentAnchor = null;
        gameObject.SetActive(false);
    }
}
