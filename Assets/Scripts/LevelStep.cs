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

    [Header("OPTIONAL ITEM REQUIREMENT TO CONTINUE")]
    public string requiredItemId;

    [Header("OPTIONAL TRIGGER REQUIREMENT TO CONTINUE")]
    public string requiredTriggerId;

    [Header("OPTIONAL QUEST TEXT")]
    public bool updateQuestText = false;

    [TextArea(2, 3)]
    public string newQuestText;

    [Header("JEKKO TYPE")]
    public JekkoType jekkoType = JekkoType.NORMAL;
}
