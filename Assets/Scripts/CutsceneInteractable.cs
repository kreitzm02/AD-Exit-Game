using System.Collections;
using DigitalRuby.Tween;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;

public class CutsceneInteractable : Interactable
{
    [Header("SEQUENCES")]
    [SerializeField] private CutsceneSequence[] sequences;

    [Header("SEQUENCE MODE")]
    [SerializeField] private InteractionSequenceMode sequenceMode = InteractionSequenceMode.SEQUENTIAL;

    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("CAMERA")]
    [SerializeField] private float zoomSize = 3.5f;

    [Header("ZOOM SETTINGS")]
    [SerializeField] private bool smoothZoom = true;
    [SerializeField] private float zoomDuration = 0.4f;

    [Header("FOCUS SETTINGS")]
    [SerializeField] private bool smoothFocus = true;
    [SerializeField] private float focusDuration = 0.4f;

    [Header("JEKKO SETTINGS")]
    [SerializeField] private bool useJekko = false;
    [SerializeField] private Transform pocketSpawnPoint;
    [SerializeField] private Transform jekkoTarget;
    [SerializeField] private GameObject jekkoGO;
    [SerializeField] private float jekkoFlyTime = 0.5f;

    [Header("CUTSCENE DELAY")]
    [SerializeField] private float cutsceneStartDelay = 0f;

    [Header("TRIGGER ID")]
    [SerializeField] private string levelTriggerId;

    private int currentSequenceIndex = 0;
    private bool isPlaying;
    private bool stepSkipRequested = false;
    private EventInstance currentAudio;
    private bool hasCurrentAudio;

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

    private void Update()
    {
        if (stepSkipRequested == false && Input.GetKeyDown(KeyCode.Space))
        {
            stepSkipRequested = true;
            Debug.Log("Step Skip Requested");
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

        if (cutsceneStartDelay > 0f)
            yield return new WaitForSeconds(cutsceneStartDelay);

        PlayerController player = FindFirstObjectByType<PlayerController>();
        PlayerCamera cam = FindFirstObjectByType<PlayerCamera>();

        player.LockInput(true);

        cam.ZoomTo(zoomSize, smoothZoom, zoomDuration);

        yield return new WaitForSeconds(zoomDuration * 0.3f);

        foreach (var step in sequence.steps)
        {
            if (step.focusTarget)
                cam.FocusOn(step.focusTarget, step.focusOffset, smoothFocus, focusDuration);
            else
                cam.ClearFocus(smoothFocus, focusDuration);

            switch (step.stepType)
            {
                case CutsceneStepType.JEKKOSPAWN:
                    SubtitleUI.Instance.Show(step.subtitleText);
                    SpawnSideCharacter();
                    StartStepAudio(step.fmodEventRef);
                    break;

                case CutsceneStepType.JEKKODESPAWN:
                    DespawnSideCharacter();
                    SubtitleUI.Instance.Show(step.subtitleText);
                    StartStepAudio(step.fmodEventRef);
                    break;

                case CutsceneStepType.DIALOGUE:
                    SubtitleUI.Instance.Show(step.subtitleText);
                    StartStepAudio(step.fmodEventRef);
                    break;
            }

            float t = 0f;
            while (t < step.duration)
            {
                if (IsSkipStepRequested())
                {
                    StopStepAudio(immediate: true);
                    break;
                }

                t += Time.deltaTime;
                yield return null;
            }
        }

        SubtitleUI.Instance.Hide();

        cam.ClearFocus(smoothFocus, focusDuration);

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

    private void StartStepAudio(EventReference eventRef)
    {
        //StopStepAudio(immediate: false);

        if (eventRef.IsNull) return;

        currentAudio = RuntimeManager.CreateInstance(eventRef);

        var pos = Camera.main ? Camera.main.transform.position : Vector3.zero;
        currentAudio.set3DAttributes(RuntimeUtils.To3DAttributes(pos));

        currentAudio.start();
        hasCurrentAudio = true;
    }

    private void StopStepAudio(bool immediate)
    {
        if (!hasCurrentAudio) return;

        currentAudio.stop(immediate ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentAudio.release();

        hasCurrentAudio = false;
    }

    private bool IsSkipStepRequested()
    {
        if (!stepSkipRequested) return false;
        stepSkipRequested = false;
        return true;
    }
}

public enum InteractionSequenceMode
{
    SEQUENTIAL,   
    RANDOM,        
}
