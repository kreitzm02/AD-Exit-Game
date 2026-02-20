using NUnit.Framework;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    [SerializeField] private bool interactableDebugCircle = false;
    [SerializeField] private bool deactivateAllInteractables = true;

    private Interactable[] allInteractables;

    private void Awake()
    {
        allInteractables = GameObject.FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var interactable in allInteractables)
        {
            SpriteRenderer sr = interactable.GetComponentInChildren<SpriteRenderer>();

            if (sr != null ) sr.enabled = interactableDebugCircle;

            if (interactable is ReadableInteractable) continue;

            interactable.gameObject.SetActive(!deactivateAllInteractables);
        }
    }
}
