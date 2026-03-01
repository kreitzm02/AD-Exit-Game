using FMODUnity;
using UnityEngine;

[System.Serializable]
public class CutsceneStep
{
    public CutsceneStepType stepType = CutsceneStepType.DIALOGUE;

    [TextArea(2, 4)]
    public string subtitleText;

    public float duration = 2f;
    public EventReference fmodEventRef;

    public Transform focusTarget;
    public Vector2 focusOffset;
}

public enum CutsceneStepType
{
    DIALOGUE,
    JEKKOSPAWN,
    JEKKODESPAWN
}
