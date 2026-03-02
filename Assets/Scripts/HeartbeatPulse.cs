using System.Collections;
using UnityEngine;


public class HeartbeatPulse : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SpriteRenderer target;

    [Header("Normal Beat (seconds)")]
    [SerializeField] private float onTime = 0.08f;
    [SerializeField] private float offTime = 0.28f;

    [Header("Humanization")]
    [Range(0f, 0.3f)]
    [SerializeField] private float jitter = 0.03f;

    [Header("Arrhythmia")]
    [Range(0f, 1f)]
    [SerializeField] private float arrhythmiaChance = 0.12f;
    [SerializeField] private bool enableExtrasystole = true;
    [SerializeField] private bool enablePause = true;
    [SerializeField] private bool enableFlutter = true;

    [Header("Extrasystole")]
    [SerializeField] private float extraOnTime = 0.05f;
    [SerializeField] private float extraOffTime = 0.12f;

    [Header("Pause")]
    [SerializeField] private float pauseExtraOff = 0.45f;

    [Header("Flutter")]
    [SerializeField] private int flutterFlickers = 5;
    [SerializeField] private float flutterOn = 0.03f;
    [SerializeField] private float flutterOff = 0.05f;

    [Header("Start")]
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private bool startVisible = false;

    private Coroutine loopCo;

    private void Awake()
    {
        if (!target) target = GetComponent<SpriteRenderer>();
        if (target) target.enabled = startVisible;

        if (startOnAwake)
            StartLoop();
    }

    private void OnEnable()
    {
        if (startOnAwake && loopCo == null)
            StartLoop();
    }

    private void OnDisable()
    {
        StopLoop();
    }

    public void StartLoop()
    {
        if (!target) return;
        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = StartCoroutine(Loop());
    }

    public void StopLoop()
    {
        if (loopCo != null)
        {
            StopCoroutine(loopCo);
            loopCo = null;
        }
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            yield return Beat(onTime, offTime);

            if (Random.value < arrhythmiaChance)
            {
                yield return ArrhythmiaEvent();
            }
        }
    }

    private IEnumerator Beat(float on, float off)
    {
        SetVisible(true);
        yield return Wait(on + RandJitter());

        SetVisible(false);
        yield return Wait(off + RandJitter());
    }

    private IEnumerator ArrhythmiaEvent()
    {
        int attempts = 0;
        while (attempts++ < 6)
        {
            int choice = Random.Range(0, 3);

            if (choice == 0 && enableExtrasystole)
            {
                yield return Wait(0.05f + RandJitter());
                yield return Beat(extraOnTime, extraOffTime);
                yield break;
            }

            if (choice == 1 && enablePause)
            {
                SetVisible(false);
                yield return Wait((offTime + pauseExtraOff) + RandJitter());
                yield break;
            }

            if (choice == 2 && enableFlutter)
            {
                int n = Mathf.Max(1, flutterFlickers + Random.Range(-2, 3));
                for (int i = 0; i < n; i++)
                {
                    yield return Beat(flutterOn, flutterOff);
                }
                yield break;
            }
        }

        yield break;
    }

    private void SetVisible(bool v)
    {
        if (target) target.enabled = v;
    }

    private float RandJitter()
    {
        if (jitter <= 0f) return 0f;
        return Random.Range(-jitter, jitter);
    }

    private static YieldInstruction Wait(float t)
    {
        if (t < 0.001f) t = 0.001f;
        return new WaitForSeconds(t);
    }
}
