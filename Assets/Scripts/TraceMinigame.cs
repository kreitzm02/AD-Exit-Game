using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class TraceMinigame : MonoBehaviour
{
    [Serializable]
    public class Stroke
    {
        public Transform start;
        public Transform end;
        [Min(0.01f)] public float tolerance = 0.15f;
    }

    [Serializable]
    public class Letter
    {
        public SpriteRenderer reveal;
        public List<Stroke> strokes = new();
        public GameObject guidesRoot;
    }

    [Header("LETTERS")]
    [SerializeField] private List<Letter> letters = new();

    [Header("INPUT")]
    [SerializeField] private Camera inputCamera;

    [Header("GUIDES")]
    [SerializeField] private bool hideFutureLettersGuides = true;

    [Header("USER TRACE")]
    [SerializeField] private LineRenderer userTraceLine;
    [SerializeField, Min(0.001f)] private float traceMinPointDistance = 0.02f;
    [SerializeField] private LineRenderer committedStrokePrefab;
    [SerializeField] private Transform committedStrokeParent;
    [SerializeField] private float userStrokesZOffset = -15.0f;

    [Header("PROGRESS")]
    [SerializeField, Range(0.8f, 1f)] private float completeThreshold = 0.98f;
    [SerializeField, Range(0f, 0.1f)] private float backtrackTolerance = 0.02f;

    [Header("FMOD")]
    [SerializeField] private EventReference drawLoopSfx;
    [SerializeField] private EventReference strokeDoneSfx;
    [SerializeField] private EventReference letterDoneSfx;

    private TraceInteractable owner;
    private int letterIndex;

    private bool isOpen;
    private bool isSolved;

    private bool isDrawing;

    private bool[] strokeCompleted;

    private int activeStrokeIndex = -1;
    private bool activeReversed;
    private float activeProgress;
    private Vector2 lastAddedPoint;

    private readonly List<LineRenderer> committedLines = new();

    private EventInstance drawLoopInstance;

    private void Awake()
    {
        if (!inputCamera) inputCamera = Camera.main;
        ResetState(keepSolvedReveal: true);
        gameObject.SetActive(false);

        ClearUserTrace();
    }

    public void Open(TraceInteractable interactable)
    {
        owner = interactable;
        isOpen = true;

        if (!isSolved)
            ResetState(keepSolvedReveal: false);

        gameObject.SetActive(true);
        UpdateGuidesVisibility();
        RefreshLetterReveal();
    }

    public void Close(bool viaBackButton)
    {
        StopDrawLoop();

        if (!isSolved)
            ResetState(keepSolvedReveal: false);

        isOpen = false;
        ClearCommittedLines();
        gameObject.SetActive(false);

        owner?.CloseFromMinigame(isSolved);
        owner = null;

        ClearUserTrace();
    }

    private void Update()
    {
        if (!isOpen || isSolved) return;

        if (Input.GetKeyDown(KeyCode.Space))
            Close(true);

        bool down = Input.GetMouseButtonDown(0);
        bool held = Input.GetMouseButton(0);
        bool up = Input.GetMouseButtonUp(0);

        if (down) BeginStrokeAttempt();
        if (held) UpdateStrokeAttempt();
        if (up) EndStrokeAttempt();
    }

    private void BeginStrokeAttempt()
    {
        if (!TryGetMouseWorld(out var p))
            return;

        if (!TryAcquireStroke(p))
            return;

        isDrawing = true;
        StartDrawLoop();

        ClearUserTrace();
        lastAddedPoint = p;
        AddUserTracePoint(p);
    }

    private void UpdateStrokeAttempt()
    {
        if (!isDrawing) return;

        if (!TryGetMouseWorld(out var p))
            return;

        if (activeStrokeIndex < 0)
        {
            if (!TryAcquireStroke(p))
                return;

            ClearUserTrace();
            lastAddedPoint = p;
            AddUserTracePoint(p);
            return;
        }

        var stroke = GetActiveStroke();
        if (stroke == null) return;

        Vector2 a = stroke.start.position;
        Vector2 b = stroke.end.position;

        float t = ProjectPoint01(p, a, b, out float dist);

        if (dist > stroke.tolerance)
            return;

        float progress = activeReversed ? (1f - t) : t;

        if (progress + backtrackTolerance < activeProgress)
            return;

        activeProgress = Mathf.Max(activeProgress, progress);

        if (Vector2.Distance(p, lastAddedPoint) >= traceMinPointDistance)
        {
            AddUserTracePoint(p);
            lastAddedPoint = p;
        }

        if (activeProgress >= completeThreshold)
            CompleteStroke();
    }

    private void EndStrokeAttempt()
    {
        isDrawing = false;
        StopDrawLoop();

        ClearUserTrace();
        activeStrokeIndex = -1;
        activeProgress = 0f;
        activeReversed = false;
    }

    private void CompleteStroke()
    {
        isDrawing = false;
        StopDrawLoop();

        if (strokeDoneSfx.IsNull == false)
            RuntimeManager.PlayOneShot(strokeDoneSfx, inputCamera.transform.position);

        if (strokeCompleted != null && activeStrokeIndex >= 0 && activeStrokeIndex < strokeCompleted.Length)
            strokeCompleted[activeStrokeIndex] = true;

        CommitCurrentUserTrace();
        ClearUserTrace();

        activeStrokeIndex = -1;
        activeProgress = 0f;
        activeReversed = false;

        if (AreAllStrokesCompleted())
        {
            var currentLetter = GetCurrentLetter();
            if (currentLetter != null && currentLetter.reveal)
                currentLetter.reveal.enabled = true;

            if (letterDoneSfx.IsNull == false)
                RuntimeManager.PlayOneShot(letterDoneSfx, inputCamera.transform.position);

            letterIndex++;

            ClearCommittedLines();

            if (letterIndex >= letters.Count)
            {
                Solve();
                return;
            }

            PrepareLetter(letterIndex);
            UpdateGuidesVisibility();
            RefreshLetterReveal();
        }
    }

    private void Solve()
    {
        isSolved = true;

        foreach (var l in letters)
            if (l.reveal) l.reveal.enabled = true;

        foreach (var l in letters)
            if (l.guidesRoot) l.guidesRoot.SetActive(false);

        ClearUserTrace();
        Close(false);
    }

    private void ResetState(bool keepSolvedReveal)
    {
        letterIndex = 0;
        isDrawing = false;

        if (!keepSolvedReveal)
            isSolved = false;

        foreach (var l in letters)
        {
            if (l.reveal)
                l.reveal.enabled = isSolved;

            if (l.guidesRoot)
                l.guidesRoot.SetActive(true);
        }

        PrepareLetter(letterIndex);
        ClearUserTrace();
        ClearCommittedLines();
    }

    private void PrepareLetter(int index)
    {
        var l = GetLetter(index);
        if (l == null)
        {
            strokeCompleted = null;
            return;
        }

        strokeCompleted = new bool[l.strokes.Count];
        activeStrokeIndex = -1;
        activeProgress = 0f;
        activeReversed = false;
    }

    private void RefreshLetterReveal()
    {
        for (int i = 0; i < letters.Count; i++)
        {
            if (letters[i].reveal == null) continue;

            if (isSolved)
            {
                letters[i].reveal.enabled = true;
            }
            else
            {
                letters[i].reveal.enabled = (i < letterIndex);
            }
        }
    }

    private void UpdateGuidesVisibility()
    {
        for (int i = 0; i < letters.Count; i++)
        {
            if (!letters[i].guidesRoot) continue;
            letters[i].guidesRoot.SetActive(i == letterIndex);
        }
    }

    private Letter GetLetter(int index)
    {
        if (index < 0 || index >= letters.Count) return null;
        return letters[index];
    }

    private Letter GetCurrentLetter() => GetLetter(letterIndex);

    private Stroke GetActiveStroke()
    {
        var l = GetCurrentLetter();
        if (l == null) return null;
        if (activeStrokeIndex < 0 || activeStrokeIndex >= l.strokes.Count) return null;
        return l.strokes[activeStrokeIndex];
    }

    private bool AreAllStrokesCompleted()
    {
        if (strokeCompleted == null) return false;
        for (int i = 0; i < strokeCompleted.Length; i++)
            if (!strokeCompleted[i]) return false;
        return true;
    }

    private bool TryAcquireStroke(Vector2 p)
    {
        var l = GetCurrentLetter();
        if (l == null) return false;

        int bestIndex = -1;
        float bestDist = float.MaxValue;
        bool bestReversed = false;
        float bestProgress = 0f;

        for (int i = 0; i < l.strokes.Count; i++)
        {
            if (strokeCompleted != null && i < strokeCompleted.Length && strokeCompleted[i])
                continue;

            var s = l.strokes[i];
            if (s == null || !s.start || !s.end) continue;

            Vector2 a = s.start.position;
            Vector2 b = s.end.position;

            float t = ProjectPoint01(p, a, b, out float dist);
            if (dist > s.tolerance) continue;

            float dStart = Vector2.Distance(p, a);
            float dEnd = Vector2.Distance(p, b);
            bool reversed = dEnd < dStart;         
            float progress = reversed ? (1f - t) : t;

            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
                bestReversed = reversed;
                bestProgress = progress;
            }
        }

        if (bestIndex < 0)
            return false;

        activeStrokeIndex = bestIndex;
        activeReversed = bestReversed;
        activeProgress = bestProgress;

        return true;
    }

    private bool TryGetMouseWorld(out Vector2 world)
    {
        world = default;
        if (!inputCamera) return false;

        Vector3 m = Input.mousePosition;
        m.z = Mathf.Abs(inputCamera.transform.position.z - transform.position.z);

        Vector3 w = inputCamera.ScreenToWorldPoint(m);
        world = w;
        return true;
    }

    private static float ProjectPoint01(Vector2 p, Vector2 a, Vector2 b, out float distance)
    {
        Vector2 ab = b - a;
        float ab2 = Vector2.Dot(ab, ab);
        if (ab2 <= 0.000001f)
        {
            distance = Vector2.Distance(p, a);
            return 0f;
        }

        float t = Vector2.Dot(p - a, ab) / ab2;
        t = Mathf.Clamp01(t);

        Vector2 closest = a + ab * t;
        distance = Vector2.Distance(p, closest);
        return t;
    }

    private void AddUserTracePoint(Vector2 p)
    {
        if (!userTraceLine) return;

        int n = userTraceLine.positionCount;
        userTraceLine.positionCount = n + 1;

        Vector3 p3 = new Vector3(p.x, p.y, userStrokesZOffset);

        userTraceLine.SetPosition(n, p3);
    }

    private void ClearUserTrace()
    {
        if (!userTraceLine) return;
        userTraceLine.positionCount = 0;
    }

    private void StartDrawLoop()
    {
        if (drawLoopSfx.IsNull) return;

        StopDrawLoop();
        drawLoopInstance = RuntimeManager.CreateInstance(drawLoopSfx);
        RuntimeManager.AttachInstanceToGameObject(drawLoopInstance, inputCamera.gameObject);
        drawLoopInstance.start();
    }

    private void StopDrawLoop()
    {
        if (!drawLoopInstance.isValid()) return;
        drawLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        drawLoopInstance.release();
    }

    private void CommitCurrentUserTrace()
    {
        if (!userTraceLine) return;

        int count = userTraceLine.positionCount;
        if (count < 2) return;

        if (!committedStrokePrefab)
        {
            var cloneGo = Instantiate(userTraceLine.gameObject, committedStrokeParent ? committedStrokeParent : transform);
            var lr = cloneGo.GetComponent<LineRenderer>();
            committedLines.Add(lr);
            return;
        }

        var parent = committedStrokeParent ? committedStrokeParent : transform;
        var lrNew = Instantiate(committedStrokePrefab, parent);

        lrNew.positionCount = count;

        for (int i = 0; i < count; i++)
            lrNew.SetPosition(i, userTraceLine.GetPosition(i));

        committedLines.Add(lrNew);
    }

    private void ClearCommittedLines()
    {
        for (int i = 0; i < committedLines.Count; i++)
        {
            if (committedLines[i] != null)
                Destroy(committedLines[i].gameObject);
        }
        committedLines.Clear();
    }
}
