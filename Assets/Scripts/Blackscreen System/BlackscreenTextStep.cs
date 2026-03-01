using UnityEngine;
using FMODUnity;

[System.Serializable]
public class BlackScreenTextStep
{
    [TextArea(2, 4)]
    public string text;

    public float fadeInDuration = 0.4f;

    public float holdDuration = 1.2f;

    public float fadeOutDuration = 0.4f;

    public EventReference fmodEventRef;
}
