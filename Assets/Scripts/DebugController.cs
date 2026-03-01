using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    [Header("DEBUG INTERACTABLES")]
    [SerializeField] private bool interactableDebugCircle = false;
    [SerializeField] private bool deactivateAllInteractables = true;
    [SerializeField] private bool deactivateAllDragZones = true;

    [HideInInspector] public List<UniqueItem_SO> items;

    private Interactable[] allInteractables;

    private DragItemZone[] allDragZones;

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

        allDragZones = GameObject.FindObjectsByType<DragItemZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var dragZone in allDragZones)
        {
            SpriteRenderer sr = dragZone.GetComponentInChildren<SpriteRenderer>();

            if (sr != null) sr.enabled = interactableDebugCircle;

            dragZone.gameObject.SetActive(!deactivateAllDragZones);
        }
    }

    public void DeactivateEverything()
    {
        foreach (var interactable in allInteractables)
        {
            SpriteRenderer sr = interactable.GetComponentInChildren<SpriteRenderer>();

            if (sr != null) sr.enabled = false;

            if (interactable is ReadableInteractable) continue;

            interactable.gameObject.SetActive(false);
        }

        foreach (var dragZone in allDragZones)
        {
            SpriteRenderer sr = dragZone.GetComponentInChildren<SpriteRenderer>();

            if (sr != null) sr.enabled = false;

            dragZone.gameObject.SetActive(false);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DebugController))]
public class DebugControllerEditor : Editor
{
    private string stepName;
    private List<string> stepNames = new();
    private string roomName;
    private List<string> roomNames = new();
    private int roomEntryPointId;

    private LevelManager levelManager;
    private RoomManager roomManager;

    private SerializedProperty itemsProp;

    public void OnEnable()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        roomManager = FindFirstObjectByType<RoomManager>();
        itemsProp = serializedObject.FindProperty("items");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        GUILayout.Space(20);
        GUILayout.Label("OVERRIDE SAVE DATA", EditorStyles.boldLabel);

        GUILayout.Label("Set Saved Step:");
        int currStepIndex = Mathf.Max(0, System.Array.IndexOf(stepNames.ToArray(), stepName));
        stepName = stepNames.ElementAt(EditorGUILayout.Popup(currStepIndex, GetStepNames()));

        GUILayout.Label("Set Saved Room:");
        int currRoomIndex = Mathf.Max(0, System.Array.IndexOf(roomNames.ToArray(), roomName));
        roomName = roomNames.ElementAt(EditorGUILayout.Popup(currRoomIndex, GetRoomNames()));

        GUILayout.Label("Set Room Entry Point ID:");
        roomEntryPointId = EditorGUILayout.IntField(roomEntryPointId);

        GUILayout.Label("Set Inventory Items:");
        EditorGUILayout.PropertyField(itemsProp, includeChildren: true);
        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(20);
        GUILayout.Label("Override save data:");

        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Please EXIT play mode to override save data!", MessageType.Warning);     
        }
        else if (GUILayout.Button("Confirm override save data"))
        {
            OverrideSaveData();
        }
    }

    private string[] GetStepNames()
    {
        stepNames.Clear();

        IReadOnlyList<LevelStep> stepsList = levelManager.GetSteps();

        foreach (LevelStep step in stepsList)
        {
            stepNames.Add(step.stepName);
        }

        return stepNames.ToArray();
    }

    private string[] GetRoomNames()
    {
        roomNames.Clear();

        IReadOnlyList<RoomData> roomsList = roomManager.Rooms;

        foreach (RoomData roomData in roomsList)
        {
            roomNames.Add(roomData.roomId);
        }

        return roomNames.ToArray();
    }

    private void OverrideSaveData()
    {
        IReadOnlyList<LevelStep> stepsList = levelManager.GetSteps();
        int stepIndex = 0;

        for(int i = 0;  i < stepsList.Count; i++)
        {
            if (stepsList.ElementAt(i).stepName == this.stepName)
            {
                stepIndex = i;
                break;
            }
        }

        IReadOnlyList<RoomData> roomsList = roomManager.Rooms;
        Vector3 entryPointPos = new();

        foreach (RoomData roomData in roomsList)
        {
            if (roomData.roomId == roomName && roomEntryPointId >= 0 && roomEntryPointId < roomData.entryPoints.Length)
            {
                entryPointPos = roomData.entryPoints[roomEntryPointId].transform.position;
                break;
            }
        }

        DebugController dc = (DebugController)target;
        List<string> itemNames = new();

        foreach(UniqueItem_SO item in dc.items)
        {
            itemNames.Add(item.itemId);
        }

        GameSaveData saveData = new GameSaveData
        {
            currentStep = stepIndex,
            currentRoomId = roomName,
            playerPosX = entryPointPos.x,
            playerPosY = entryPointPos.y,
            playerPosZ = entryPointPos.z,
            inventoryItemIds = itemNames
        };

        GameSaveSystem.Save(saveData);
    }
}

#endif

