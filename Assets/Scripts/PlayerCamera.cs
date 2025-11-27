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
        if (!target) return;

        Vector3 targetPos = target.position + (Vector3)offset;
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
        if (!target) return;

        Vector3 snapPos = target.position + (Vector3)offset;
        snapPos.z = transform.position.z;

        transform.position = snapPos;

        velocity = Vector3.zero;

        noiseTime = 0f;
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
