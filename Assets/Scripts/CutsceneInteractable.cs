using DigitalRuby.Tween;
using FMODUnity;
using System.Collections;
using UnityEngine;

public class CutsceneInteractable : Interactable
{
    [Header("SEQUENCES")]
    [SerializeField] private CutsceneSequence[] sequences;

    [Header("SEQUENCE MODE")]
    [SerializeField] private InteractionSequenceMode sequenceMode = InteractionSequenceMode.SEQUENTIAL;

    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("CAMERA")]
    [SerializeField] private Transform focusPoint;
    [SerializeField] private float zoomSize = 3.5f;

    [Header("ZOOM SETTINGS")]
    [SerializeField] private bool smoothZoom = true;
    [SerializeField] private float zoomDuration = 0.4f;

    [Header("JEKKO SETTINGS")]
    [SerializeField] private bool useJekko = false;
    [SerializeField] private Transform pocketSpawnPoint;
    [SerializeField] private Transform jekkoTarget;
    [SerializeField] private GameObject jekkoGO;
    [SerializeField] private float jekkoFlyTime = 0.5f;

    [Header("TRIGGER ID")]
    [SerializeField] private string levelTriggerId;

    private int currentSequenceIndex = 0;
    private bool isPlaying;

    private Tween<Vector3> jekkoMoveTween;
    private Tween<Vector3> jekkoScaleTween;

    public override void OnEnterRange()
    {
        base.OnEnterRange();

        if (triggerMode == InteractionTriggerMode.AUTO)
        {
            TryPlay();
        }
    }

    private void TryPlay()
    {
        if (!isPlayerInRange || isPlaying || sequences.Length == 0)
            return;

        CutsceneSequence selectedSequence = GetNextSequence();

        OnExitRange();
        StartCoroutine(PlayCutscene(selectedSequence));
    }

    public override void Interact()
    {
        if (triggerMode != InteractionTriggerMode.MANUAL)
            return;

        TryPlay();
    }

    private CutsceneSequence GetNextSequence()
    {
        if (sequenceMode == InteractionSequenceMode.RANDOM)
        {
            return sequences[Random.Range(0, sequences.Length)];
        }

        CutsceneSequence seq = sequences[currentSequenceIndex];

        currentSequenceIndex++;

        if (currentSequenceIndex >= sequences.Length)
            currentSequenceIndex = sequences.Length - 1;

        return seq;
    }

    private IEnumerator PlayCutscene(CutsceneSequence sequence)
    {
        isPlaying = true;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        PlayerCamera cam = FindFirstObjectByType<PlayerCamera>();

        player.LockInput(true);

        cam.ZoomTo(zoomSize, smoothZoom, zoomDuration);

        yield return new WaitForSeconds(zoomDuration * 0.3f);

        if (useJekko)
            SpawnSideCharacter();

        foreach (var step in sequence.steps)
        {
            SubtitleUI.Instance.Show(step.subtitleText);
            PlayFMOD(step.fmodEventRef);

            yield return new WaitForSeconds(step.duration);
        }

        if (useJekko)
            DespawnSideCharacter();

        SubtitleUI.Instance.Hide();
        cam.ResetZoom(smoothZoom, zoomDuration);
        player.LockInput(false);

        if (!string.IsNullOrEmpty(levelTriggerId))
        {
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);
        }

        isPlaying = false;
    }

    private void SpawnSideCharacter()
    {
        jekkoGO.SetActive(true);
        jekkoGO.transform.position = pocketSpawnPoint.position;
        jekkoGO.transform.localScale = Vector3.zero;

        JekkoFloat floatComp = jekkoGO.GetComponent<JekkoFloat>();
        if (!floatComp)
            jekkoGO.AddComponent<JekkoFloat>();

        jekkoMoveTween = gameObject.Tween(
            "JekkoMoveIn",
            jekkoGO.transform.position,
            jekkoTarget.position,
            jekkoFlyTime,
            TweenScaleFunctions.QuadraticEaseOut,
            t =>
            {
                jekkoGO.transform.position = t.CurrentValue;
            }
        );

        jekkoScaleTween = gameObject.Tween(
            "JekkoScaleIn",
            Vector3.zero,
            Vector3.one,
            jekkoFlyTime,
            TweenScaleFunctions.QuadraticEaseOut,
            t =>
            {
                jekkoGO.transform.localScale = t.CurrentValue;
            }
        );
    }

    private void DespawnSideCharacter()
    {
        jekkoMoveTween = gameObject.Tween(
            "JekkoMoveOut",
            jekkoGO.transform.position,
            pocketSpawnPoint.position,
            jekkoFlyTime,
            TweenScaleFunctions.QuadraticEaseIn,
            t =>
            {
                jekkoGO.transform.position = t.CurrentValue;
            },
            t =>
            {
                jekkoGO.SetActive(false);
            }
        );

        jekkoScaleTween = gameObject.Tween(
            "JekkoScaleOut",
            Vector3.one,
            Vector3.zero,
            jekkoFlyTime,
            TweenScaleFunctions.QuadraticEaseIn,
            t =>
            {
                jekkoGO.transform.localScale = t.CurrentValue;
            }
        );
    }

    private void PlayFMOD(EventReference eventRef)
    {
        if (eventRef.IsNull)
            return;

        FMODUnity.RuntimeManager.PlayOneShot(eventRef);
    }
}

public enum InteractionSequenceMode
{
    SEQUENTIAL,   
    RANDOM,        
}
