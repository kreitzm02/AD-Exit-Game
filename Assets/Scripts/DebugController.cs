using NUnit.Framework;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    [SerializeField] private bool interactableDebug = false;

    private Interactable[] allInteractables;

    private void Start()
    {
        allInteractables = GameObject.FindObjectsByType<Interactable>(FindObjectsSortMode.None);

        foreach (var interactable in allInteractables)
        {
            SpriteRenderer sr = interactable.GetComponentInChildren<SpriteRenderer>();

            if (sr != null ) sr.enabled = interactableDebug;
        }
    }
}
