using UnityEngine;

[System.Serializable]
public class LevelStep
{
    [Header("DEBUG NAME")]
    public string stepName;

    [Header("ACTIVATE THESE OBJECTS")]
    public GameObject[] activateObjects;

    [Header("DEACTIVATE THESE OBJECTS")]
    public GameObject[] deactivateObjects;

    [Header("OPTIONAL ITEM REQUIREMENT")]
    public string requiredItemId;

    [Header("OPTIONAL TRIGGER REQUIREMENT")]
    public string requiredTriggerId;
}
