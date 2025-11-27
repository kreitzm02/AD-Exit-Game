using FMODUnity;
using UnityEngine;

[System.Serializable]
public class CutsceneStep
{
    [TextArea(2, 4)]
    public string subtitleText;
    public float duration = 2f;

    [Header("FMOD Audio - example: event:/Dialogue/Narrator_01")]
    public string fmodEventPath;
    public EventReference startMusic;
}
