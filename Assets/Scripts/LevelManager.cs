using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("LEVEL STEPS")]
    [SerializeField] private List<LevelStep> steps = new List<LevelStep>();

    [Header("START MODE")]
    [SerializeField] private bool debugBypassLevelSystem = false;

    private int currentStepIndex = -1;   

    private HashSet<string> completedTriggers = new HashSet<string>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!debugBypassLevelSystem)
            StartLevelFromBeginning();
    }

    public void StartLevelFromBeginning()
    {
        if (debugBypassLevelSystem)
        {
            Debug.Log("[LevelManager] DEBUG MODE ACTIVE → LevelSystem bypassed.");
            return;
        }

        completedTriggers.Clear();
        currentStepIndex = 0;

        ApplyCurrentStep();
    }

    private void Update()
    {
        if (debugBypassLevelSystem)
            return;

        if (currentStepIndex < 0 || currentStepIndex >= steps.Count)
            return;

        CheckStepProgress();
    }

    private void ApplyCurrentStep()
    {
        if (currentStepIndex < 0 || currentStepIndex >= steps.Count)
            return;

        LevelStep step = steps[currentStepIndex];

        foreach (var go in step.activateObjects)
            if (go) go.SetActive(true);

        foreach (var go in step.deactivateObjects)
            if (go) go.SetActive(false);

        Debug.Log("[LevelManager] Step Activated: " + step.stepName);
    }

    private void CheckStepProgress()
    {
        LevelStep step = steps[currentStepIndex];

        if (!string.IsNullOrEmpty(step.requiredItemId))
        {
            if (!PlayerInventory.Instance.HasItem(step.requiredItemId))
                return;
        }

        if (!string.IsNullOrEmpty(step.requiredTriggerId))
        {
            if (!completedTriggers.Contains(step.requiredTriggerId))
                return;
        }

        AdvanceStep();
    }

    private void AdvanceStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= steps.Count)
        {
            Debug.Log("[LevelManager] LEVEL COMPLETED");
            return;
        }

        ApplyCurrentStep();
    }

    public void NotifyTriggerCompleted(string triggerId)
    {
        if (debugBypassLevelSystem)
            return;

        if (!completedTriggers.Contains(triggerId))
        {
            completedTriggers.Add(triggerId);
            Debug.Log("[LevelManager] Trigger completed: " + triggerId);
        }
    }
}
