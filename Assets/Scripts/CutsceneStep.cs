using FMODUnity;
using UnityEngine;

[System.Serializable]
public class CutsceneStep
{
    [TextArea(2, 4)]
    public string subtitleText;
    public float duration = 2f;
    public EventReference fmodEventRef;
}
