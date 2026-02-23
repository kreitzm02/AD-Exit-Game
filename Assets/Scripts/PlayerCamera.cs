using DigitalRuby.Tween;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("TARGET")]
    [SerializeField] private Transform target;

    [Header("FOLLOW SETTINGS")]
    [SerializeField] private float smoothTime = 0.25f;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float defaultZoom = 7f;

    [Header("BOUNDS")]
    [SerializeField] private BoxCollider2D cameraBounds;

    [Header("CAMERA NOISE")]
    [SerializeField] private bool enableNoise = true;
    [SerializeField] private float noiseStrength = 0.08f;
    [SerializeField] private float noiseSpeed = 0.4f;

    [Header("BOUNDS RETURN")]
    [SerializeField] private float boundsReturnDuration = 0.25f;

    private Vector3 velocity;
    private Tween<float> zoomTween;

    private float minX;
    private float maxX;
    private float halfWidth;
    private float noiseTime;

    private Transform overrideTarget;
    private Vector2 overrideOffset;

    private bool cameraBoundsActive = true;
    private bool followSuspended;

    private Tween<Vector3> moveTween;
    private Tween<float> returnTween;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;

        RefreshHalfWidthFromCurrentZoom();
        RecalculateBounds();
    }

    private void FixedUpdate()
    {
        if (followSuspended) return;

        Transform currentTarget = overrideTarget ? overrideTarget : target;
        if (!currentTarget) return;

        Vector2 extra = overrideTarget ? overrideOffset : Vector2.zero;

        Vector3 targetPos = currentTarget.position + (Vector3)(offset + extra);
        targetPos.z = transform.position.z;

        Vector3 smoothPos = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            smoothTime
        );

        if (cameraBounds && cameraBoundsActive)
        {
            smoothPos.x = Mathf.Clamp(smoothPos.x, minX, maxX);
        }

        if (enableNoise)
        {
            noiseTime += Time.fixedDeltaTime * noiseSpeed;

            float swayX = (Mathf.PerlinNoise(noiseTime, 0f) - 0.5f) * noiseStrength;
            float swayY = (Mathf.PerlinNoise(0f, noiseTime) - 0.5f) * noiseStrength;

            smoothPos += new Vector3(swayX, swayY, 0f);
        }

        transform.position = smoothPos;
    }

    public void SetNewBounds(BoxCollider2D newBounds)
    {
        cameraBounds = newBounds;
        RecalculateBounds();
    }

    public void SetCameraBoundsActive(bool value)
    {
        SetCameraBoundsActive(value, smoothReturn: true, duration: boundsReturnDuration);
    }

    public void SetCameraBoundsActive(bool value, bool smoothReturn, float duration)
    {
        if (!value)
        {
            cameraBoundsActive = false;
            return;
        }

        if (!cameraBounds)
        {
            cameraBoundsActive = true;
            return;
        }

        RefreshHalfWidthFromCurrentZoom();
        RecalculateBounds();

        Vector3 start = transform.position;
        Vector3 clamped = ClampToBoundsForCurrentZoom(start);

        if (!smoothReturn || duration <= 0f || Mathf.Approximately(start.x, clamped.x))
        {
            cameraBoundsActive = true;
            return;
        }

        moveTween?.Stop(TweenStopBehavior.DoNotModify);
        followSuspended = true;
        cameraBoundsActive = false;

        moveTween = gameObject.Tween(
            "CameraReturnToBounds",
            start,
            clamped,
            duration,
            TweenScaleFunctions.QuadraticEaseOut,
            tw => { transform.position = tw.CurrentValue; },
            tw =>
            {
                followSuspended = false;
                velocity = Vector3.zero;
                cameraBoundsActive = true;
            }
        );
    }

    private void RefreshHalfWidthFromCurrentZoom()
    {
        if (!cam) cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
        if (!cam) return;

        halfWidth = cam.orthographicSize * cam.aspect;
    }

    private void RecalculateBounds()
    {
        if (!cameraBounds) return;

        minX = cameraBounds.bounds.min.x + halfWidth;
        maxX = cameraBounds.bounds.max.x - halfWidth;
    }

    private Vector3 ClampToBoundsForCurrentZoom(Vector3 pos)
    {
        if (!cameraBounds) return pos;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        return pos;
    }

    private float ClampXForZoom(float x, float zoom)
    {
        if (!cameraBounds || !cam) return x;

        float hw = zoom * cam.aspect;
        float min = cameraBounds.bounds.min.x + hw;
        float max = cameraBounds.bounds.max.x - hw;
        return Mathf.Clamp(x, min, max);
    }

    public void SnapToTarget()
    {
        Transform currentTarget = overrideTarget ? overrideTarget : target;
        if (!currentTarget) return;

        Vector2 extra = overrideTarget ? overrideOffset : Vector2.zero;

        Vector3 snapPos = currentTarget.position + (Vector3)(offset + extra);
        snapPos.z = transform.position.z;

        transform.position = snapPos;

        velocity = Vector3.zero;
        noiseTime = 0f;
    }

    public void FocusOn(Transform focus, Vector2 focusOffset, bool smooth, float duration = 0.4f)
    {
        overrideTarget = focus;
        overrideOffset = focusOffset;

        if (!focus) return;

        MoveToTarget(focus, smooth, duration);
    }

    public void ClearFocus(bool smooth, float duration = 0.4f)
    {
        overrideTarget = null;
        overrideOffset = Vector2.zero;

        if (!target) return;

        MoveToTarget(target, smooth, duration);
    }

    public void ClearFocusInstant()
    {
        overrideTarget = null;
        overrideOffset = Vector2.zero;
        velocity = Vector3.zero;
    }

    private void MoveToTarget(Transform t, bool smooth, float duration)
    {
        if (!t) return;

        Vector2 extra = overrideTarget ? overrideOffset : Vector2.zero;

        Vector3 targetPos = t.position + (Vector3)(offset + extra);
        targetPos.z = transform.position.z;

        if (!smooth || duration <= 0f)
        {
            moveTween?.Stop(TweenStopBehavior.DoNotModify);
            followSuspended = false;
            transform.position = targetPos;
            velocity = Vector3.zero;
            return;
        }

        moveTween?.Stop(TweenStopBehavior.DoNotModify);

        followSuspended = true;

        moveTween = gameObject.Tween(
            "CameraMoveToTarget",
            transform.position,
            targetPos,
            duration,
            TweenScaleFunctions.QuadraticEaseOut,
            tw => { transform.position = tw.CurrentValue; },
            tw =>
            {
                followSuspended = false;
                velocity = Vector3.zero;
            }
        );
    }

    public void ZoomTo(float targetZoom, bool smooth, float duration = 0.4f)
    {
        if (!cam) cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
        if (!cam) return;

        if (!smooth)
        {
            cam.orthographicSize = targetZoom;
            RefreshHalfWidthFromCurrentZoom();
            RecalculateBounds();
            return;
        }

        zoomTween?.Stop(TweenStopBehavior.DoNotModify);

        zoomTween = gameObject.Tween(
            "CameraZoom",
            cam.orthographicSize,
            targetZoom,
            duration,
            TweenScaleFunctions.QuadraticEaseOut,
            t =>
            {
                cam.orthographicSize = t.CurrentValue;
                RefreshHalfWidthFromCurrentZoom();
                RecalculateBounds();
            }
        );
    }

    public void ResetZoom(bool smooth, float duration = 0.4f)
    {
        ZoomTo(defaultZoom, smooth, duration);
    }

    public void ReturnToPlayerAndBounds(bool smooth, float duration)
    {
        if (!cam) cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
        if (!cam) return;

        moveTween?.Stop(TweenStopBehavior.DoNotModify);
        zoomTween?.Stop(TweenStopBehavior.DoNotModify);
        returnTween?.Stop(TweenStopBehavior.DoNotModify);

        followSuspended = true;
        cameraBoundsActive = false;

        if (!target)
        {
            ReturnToBoundsOnly(smooth, duration);
            return;
        }

        float startZoom = cam.orthographicSize;
        float endZoom = defaultZoom;

        Vector3 startPos = transform.position;

        Vector3 desiredEndPos = target.position + (Vector3)offset;
        desiredEndPos.z = startPos.z;

        desiredEndPos.x = ClampXForZoom(desiredEndPos.x, endZoom);

        if (!smooth || duration <= 0f)
        {
            cam.orthographicSize = endZoom;
            transform.position = desiredEndPos;

            RefreshHalfWidthFromCurrentZoom();
            RecalculateBounds();

            velocity = Vector3.zero;
            followSuspended = false;
            cameraBoundsActive = true;
            return;
        }

        returnTween = gameObject.Tween(
            "CameraReturnToPlayerAndBounds",
            0f,
            1f,
            duration,
            TweenScaleFunctions.QuadraticEaseOut,
            tw =>
            {
                float s = tw.CurrentValue;

                cam.orthographicSize = Mathf.Lerp(startZoom, endZoom, s);
                transform.position = Vector3.Lerp(startPos, desiredEndPos, s);
            },
            tw =>
            {
                cam.orthographicSize = endZoom;
                transform.position = desiredEndPos;

                RefreshHalfWidthFromCurrentZoom();
                RecalculateBounds();

                velocity = Vector3.zero;
                followSuspended = false;
                cameraBoundsActive = true;
            }
        );
    }

    private void ReturnToBoundsOnly(bool smooth, float duration)
    {
        float startZoom = cam.orthographicSize;
        float endZoom = defaultZoom;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos;
        endPos.x = ClampXForZoom(startPos.x, endZoom);

        if (!smooth || duration <= 0f)
        {
            cam.orthographicSize = endZoom;
            transform.position = endPos;
            RefreshHalfWidthFromCurrentZoom();
            RecalculateBounds();
            followSuspended = false;
            cameraBoundsActive = true;
            return;
        }

        returnTween = gameObject.Tween(
            "CameraReturnBoundsOnly",
            0f,
            1f,
            duration,
            TweenScaleFunctions.QuadraticEaseOut,
            tw =>
            {
                float s = tw.CurrentValue;
                cam.orthographicSize = Mathf.Lerp(startZoom, endZoom, s);
                transform.position = Vector3.Lerp(startPos, endPos, s);
            },
            tw =>
            {
                cam.orthographicSize = endZoom;
                transform.position = endPos;
                RefreshHalfWidthFromCurrentZoom();
                RecalculateBounds();
                followSuspended = false;
                cameraBoundsActive = true;
            }
        );
    }
}
