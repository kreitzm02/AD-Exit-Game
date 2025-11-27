using UnityEngine;

[System.Serializable]
public class CutsceneSequence
{
    [Header("SEQUENCE NAME")]
    public string sequenceName;

    [Header("CUTSCENE STEPS")]
    public CutsceneStep[] steps;
}
