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

    private Vector3 velocity;

    private Tween<float> zoomTween;

    private float minX;
    private float maxX;
    private float halfWidth;
    private float noiseTime;

    private Transform overrideTarget;

    private Vector2 overrideOffset;

    private bool followSuspended;
    private Tween<Vector3> moveTween;

    private void Start()
    {
        Camera cam = Camera.main;
        halfWidth = cam.orthographicSize * cam.aspect;

        if (cameraBounds)
        {
            minX = cameraBounds.bounds.min.x + halfWidth;
            maxX = cameraBounds.bounds.max.x - halfWidth;
        }
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

        if (cameraBounds)
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

    private void RecalculateBounds()
    {
        if (!cameraBounds) return;

        minX = cameraBounds.bounds.min.x + halfWidth;
        maxX = cameraBounds.bounds.max.x - halfWidth;
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

        if (!focus)
            return;

        MoveToTarget(focus, smooth, duration);
    }

    public void ClearFocus(bool smooth, float duration = 0.4f)
    {
        overrideTarget = null;
        overrideOffset = Vector2.zero;

        if (!target)
            return;

        MoveToTarget(target, smooth, duration);
    }

    private void MoveToTarget(Transform t, bool smooth, float duration)
    {
        if (!t)
            return;

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
            tw =>
            {
                transform.position = tw.CurrentValue;
            },
            tw =>
            {
                followSuspended = false;
                velocity = Vector3.zero;
            }
        );
    }

    public void ZoomTo(float targetZoom, bool smooth, float duration = 0.4f)
    {
        Camera cam = GetComponent<Camera>();

        if (!smooth)
        {
            cam.orthographicSize = targetZoom;
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
            }
        );
    }

    public void ResetZoom(bool smooth, float duration = 0.4f)
    {
        ZoomTo(defaultZoom, smooth, duration);
    }
}
