using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;

public class SpriteMoverInteractable : Interactable
{
    public enum InteractionTriggerMode { MANUAL, AUTO }

    [Header("TRIGGER MODE")]
    [SerializeField] private InteractionTriggerMode triggerMode = InteractionTriggerMode.MANUAL;

    [Header("SPAWN / MOVE")]
    [SerializeField] private GameObject spritePrefab;
    [SerializeField] private Transform positionA;
    [SerializeField] private Transform positionB;

    [SerializeField] private float moveDuration = 1.0f;

    [SerializeField] private float startDelay = 0.0f;

    [SerializeField] private float despawnDelay = 0.0f;

    [Header("OPTIONAL FADE")]
    [SerializeField] private bool fadeInOnSpawn = false;
    [SerializeField] private float fadeInDuration = 0.25f;

    [SerializeField] private bool fadeOutOnDespawn = false;
    [SerializeField] private float fadeOutDuration = 0.25f;

    [Header("OPTIONAL FMOD")]
    [SerializeField] private EventReference startMoveAudio;
    [SerializeField] private bool stopAudioOnFinish = true;

    [Header("LEVEL TRIGGER (OPTIONAL)")]
    [SerializeField] private string levelTriggerId;

    private bool isRunning;

    public override void OnEnterRange()
    {
        base.OnEnterRange();
        if (triggerMode == InteractionTriggerMode.AUTO)
            TryExecute();
    }

    public override void Interact()
    {
        if (triggerMode != InteractionTriggerMode.MANUAL)
            return;

        TryExecute();
    }

    private void TryExecute()
    {
        if (!isPlayerInRange || isRunning)
            return;

        if (spritePrefab == null || positionA == null || positionB == null)
        {
            Debug.LogError("[SpriteMoverInteractable] Missing references (prefab / positionA / positionB).");
            return;
        }

        OnExitRange();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        isRunning = true;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        player.LockInput(true);

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        var spawned = Instantiate(spritePrefab, positionA.position, positionA.rotation);

        var renderers = spawned.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        if (fadeInOnSpawn && renderers.Length > 0)
            SetAlpha(renderers, 0f);

        AudioManager.Instance.PlaySFX(startMoveAudio);

        if (fadeInOnSpawn && renderers.Length > 0 && fadeInDuration > 0f)
            yield return FadeAlpha(renderers, 0f, 1f, fadeInDuration);

        float t = 0f;
        float dur = Mathf.Max(0.0001f, moveDuration);

        Vector3 from = positionA.position;
        Vector3 to = positionB.position;

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float eased = Mathf.Clamp01(t);

            spawned.transform.position = Vector3.Lerp(from, to, eased);

            yield return null;
        }

        if (despawnDelay > 0f)
            yield return new WaitForSeconds(despawnDelay);

        if (fadeOutOnDespawn && renderers.Length > 0)
        {
            if (fadeOutDuration > 0f)
                yield return FadeAlpha(renderers, 1f, 0f, fadeOutDuration);
            else
                SetAlpha(renderers, 0f);
        }

        Destroy(spawned);

        if (!string.IsNullOrEmpty(levelTriggerId) && LevelManager.Instance != null)
            LevelManager.Instance.NotifyTriggerCompleted(levelTriggerId);

        player.LockInput(false);

        isRunning = false;
    }

    private static void SetAlpha(SpriteRenderer[] renderers, float a)
    {
        a = Mathf.Clamp01(a);
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;
            var c = r.color;
            c.a = a;
            r.color = c;
        }
    }

    private static IEnumerator FadeAlpha(SpriteRenderer[] renderers, float from, float to, float duration)
    {
        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t));
            SetAlpha(renderers, a);
            yield return null;
        }

        SetAlpha(renderers, to);
    }
}
