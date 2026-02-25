using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("LEVEL STEPS")]
    [SerializeField] private List<LevelStep> steps = new List<LevelStep>();

    [Header("START MODE")]
    [SerializeField] private bool debugBypassLevelSystem = false;

    [Header("SIDE CHARACTER")]
    [SerializeField] private JekkoFloat sideChar;

    [Header("SAVE / LOAD")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool autoLoadOnStart = false;
    [SerializeField] private List<UniqueItem_SO> allUniqueItemsDatabase = new List<UniqueItem_SO>();

    [Header("DEBUG START (Inspector)")]
    [SerializeField] private bool useDebugStart = false;
    [SerializeField] private int debugStartStepIndex = 0;
    [SerializeField] private string debugStartRoomId = "";
    [SerializeField] private Vector3 debugStartPlayerPosition;
    [SerializeField] private List<UniqueItem_SO> debugStartItems = new List<UniqueItem_SO>();

    private int currentStepIndex = -1;
    private HashSet<string> completedTriggers = new HashSet<string>();

    private void Awake()
    {
        Instance = this;
    }

    public void StartGame()
    {
        if (debugBypassLevelSystem)
            return;

        if (useDebugStart)
        {
            StartDebugFromInspector();
            return;
        }

        if (autoLoadOnStart && GameSaveSystem.HasSave())
        {
            ContinueGameFromSave();
            return;
        }

        StartLevelFromBeginning();
    }

    public void StartGameFromSave()
    {
        if (GameSaveSystem.HasSave())
        {
            ContinueGameFromSave();
        }
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

        if (sideChar) sideChar.SetJekkoType(step.jekkoType);

        if (step.updateQuestText && !string.IsNullOrEmpty(step.newQuestText))
        {
            QuestCollapse qc = FindFirstObjectByType<QuestCollapse>();
            if (qc != null)
            {
                qc.SetQuestText(step.newQuestText);
                qc.ShowQuestBar();
            }
        }

        Debug.Log("[LevelManager] Step Activated: " + step.stepName);

        SaveCurrentGame();
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

    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }

    public IReadOnlyList<LevelStep> GetSteps()
    {
        return steps;
    }

    public void SaveCurrentGame()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("[LevelManager] Cannot save: PlayerInventory.Instance is null");
            return;
        }

        if (RoomManager.Instance == null)
        {
            Debug.LogError("[LevelManager] Cannot save: RoomManager.Instance is null");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("[LevelManager] Cannot save: playerTransform not assigned");
            return;
        }

        var save = new GameSaveData
        {
            currentStep = Mathf.Max(0, currentStepIndex),
            currentRoomId = RoomManager.Instance.GetCurrentRoomId(),
            playerPosX = playerTransform.position.x,
            playerPosY = playerTransform.position.y,
            playerPosZ = playerTransform.position.z,
            inventoryItemIds = PlayerInventory.Instance.GetAllItemIds()
        };

        GameSaveSystem.Save(save);
    }

    public void StartNewGameAndOverwriteSave()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.ClearInventory();

        completedTriggers.Clear();
        currentStepIndex = 0;

        SaveCurrentGame();
        ApplyCurrentStep();
    }

    public void ContinueGameFromSave()
    {
        var save = GameSaveSystem.Load();

        if (save == null)
        {
            Debug.LogWarning("[LevelManager] No save found. Starting new game.");
            StartLevelFromBeginning();
            return;
        }

        ApplyLoadedSave(save);
    }

    private void ApplyLoadedSave(GameSaveData save)
    {
        RestoreInventoryFromSaveIds(save.inventoryItemIds);

        if (RoomManager.Instance != null)
        {
            Vector3 pos = new Vector3(save.playerPosX, save.playerPosY, save.playerPosZ);

            bool roomLoaded = RoomManager.Instance.LoadRoomByIdAndPositionNoFade(save.currentRoomId, pos);
            if (!roomLoaded)
                Debug.LogWarning("[LevelManager] Could not load saved room. Scene default room remains active.");
        }

        RestoreLevelToStep(save.currentStep);

        Debug.Log($"[LevelManager] Continue loaded. Step={save.currentStep}, Room={save.currentRoomId}, Items={save.inventoryItemIds?.Count ?? 0}");
    }

    private void RestoreInventoryFromSaveIds(List<string> savedIds)
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("[LevelManager] Cannot restore inventory: PlayerInventory.Instance is null");
            return;
        }

        List<UniqueItem_SO> resolvedItems = new List<UniqueItem_SO>();

        if (savedIds != null)
        {
            foreach (var id in savedIds)
            {
                if (string.IsNullOrEmpty(id))
                    continue;

                var item = allUniqueItemsDatabase.FirstOrDefault(x => x != null && x.itemId == id);
                if (item != null)
                {
                    resolvedItems.Add(item);
                }
                else
                {
                    Debug.LogWarning("[LevelManager] Saved itemId not found in database: " + id);
                }
            }
        }

        PlayerInventory.Instance.RestoreInventory(resolvedItems, false);
    }

    public void RestoreLevelToStep(int targetStep)
    {
        if (debugBypassLevelSystem)
        {
            Debug.LogWarning("[LevelManager] RestoreLevelToStep ignored because debugBypassLevelSystem is true.");
            return;
        }

        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("[LevelManager] No steps configured.");
            return;
        }

        completedTriggers.Clear();

        targetStep = Mathf.Clamp(targetStep, 0, steps.Count - 1);

        currentStepIndex = 0;
        ApplyCurrentStep();

        for (int i = 1; i <= targetStep; i++)
        {
            currentStepIndex = i;
            ApplyCurrentStep();
        }

        Debug.Log("[LevelManager] Restored internally to step index: " + currentStepIndex);
    }

    [ContextMenu("DEBUG START (write save + continue)")]
    public void StartDebugFromInspector()
    {
        if (debugBypassLevelSystem)
        {
            Debug.LogWarning("[LevelManager] debugBypassLevelSystem is enabled. Debug start won't run level system.");
            return;
        }

        var data = new GameSaveData();
        data.currentStep = Mathf.Clamp(debugStartStepIndex, 0, Mathf.Max(0, steps.Count - 1));

        if (string.IsNullOrWhiteSpace(debugStartRoomId))
        {
            if (RoomManager.Instance != null)
                data.currentRoomId = RoomManager.Instance.GetCurrentRoomId();
            else
                data.currentRoomId = "";
        }
        else
        {
            data.currentRoomId = debugStartRoomId;
        }

        data.playerPosX = debugStartPlayerPosition.x;
        data.playerPosY = debugStartPlayerPosition.y;
        data.playerPosZ = debugStartPlayerPosition.z;

        data.inventoryItemIds = new List<string>();
        foreach (var item in debugStartItems)
        {
            if (item == null) continue;
            if (!data.inventoryItemIds.Contains(item.itemId))
                data.inventoryItemIds.Add(item.itemId);
        }

        GameSaveSystem.Save(data);

        ContinueGameFromSave();
    }
}
