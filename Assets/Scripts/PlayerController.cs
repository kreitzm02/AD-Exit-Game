using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("MOVEMENT")]
    [SerializeField] private float moveSpeed = 3.0f;

    [Header("ANIMATION")]
    [SerializeField] private Sprite[] idleSprites;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField, Min(1f)] private float walkFps = 10f;
    [SerializeField, Min(1f)] private float idleFps = 10f;

    [Header("FOOTSTEPS")]
    [SerializeField] private EventReference[] footstepEvents;
    [SerializeField, Min(0.01f)] private float baseFootstepInterval = 0.35f;
    [SerializeField] private Vector2 footstepIntervalRandomMultiplier = new Vector2(0.9f, 1.1f);
    [SerializeField, Range(0f, 1f)] private float footstepInputThreshold = 0.1f;
    [SerializeField, Min(0.01f)] private float minPitch = 0.9f;
    [SerializeField, Min(0.01f)] private float maxPitch = 1.1f;
    [SerializeField, Range(0.01f, 1.0f)] private float footstepVolume = 1.0f;

    [Header("INPUT")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;

    private float input;
    private bool isMoving;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private readonly HashSet<Interactable> interactablesInRange = new HashSet<Interactable>();
    private Interactable currentInteractable;

    private int walkFrameIndex;
    private float walkFrameTimer;

    private int idleFrameIndex;
    private float idleFrameTimer;

    private float footstepTimer;
    private float nextFootstepInterval;

    public bool InputLocked { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();

        interactAction.action.Enable();
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();

        interactAction.action.performed -= OnInteract;
        interactAction.action.Disable();

        ForceExitAllInteractables();
    }

    private void Start()
    {
        ResetFootstepTimer();
        UpdateCurrentInteractable();
    }

    private void Update()
    {
        ReadInput();
        HandleSpriteAnimation();
        HandleFootsteps();
        HandleFlip();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    public void LockInput(bool value)
    {
        InputLocked = value;

        if (InputLocked)
        {
            input = 0f;
            isMoving = false;
            //SetIdleSprite();
            ResetFootstepTimer();
        }
    }

    private void ReadInput()
    {
        if (InputLocked)
        {
            input = 0f;
            isMoving = false;
            return;
        }

        input = moveAction.action.ReadValue<float>();
        isMoving = Mathf.Abs(input) > 0.01f;
    }

    private void HandleMovement()
    {
        if (!isMoving) return;

        Vector2 targetPos = rb.position + new Vector2(input * moveSpeed * Time.deltaTime, 0f);
        rb.MovePosition(targetPos);
    }

    private void HandleSpriteAnimation()
    {
        if (!spriteRenderer) return;

        if (isMoving && walkSprites != null && walkSprites.Length > 0)
        {
            idleFrameTimer = 0f;
            idleFrameIndex = 0;

            walkFrameTimer += Time.deltaTime;
            float frameDuration = 1f / walkFps;

            while (walkFrameTimer >= frameDuration)
            {
                walkFrameTimer -= frameDuration;
                walkFrameIndex = (walkFrameIndex + 1) % walkSprites.Length;
            }

            Sprite s = walkSprites[walkFrameIndex];
            if (s) spriteRenderer.sprite = s;

            return;
        }

        walkFrameTimer = 0f;
        walkFrameIndex = 0;

        if (idleSprites == null || idleSprites.Length == 0)
        {
            return;
        }

        idleFrameTimer += Time.deltaTime;
        float idleFrameDuration = 1f / idleFps;

        while (idleFrameTimer >= idleFrameDuration)
        {
            idleFrameTimer -= idleFrameDuration;
            idleFrameIndex = (idleFrameIndex + 1) % idleSprites.Length;
        }

        Sprite idle = idleSprites[idleFrameIndex];
        if (idle) spriteRenderer.sprite = idle;
    }

    private void HandleFootsteps()
    {
        if (!isMoving || Mathf.Abs(input) < footstepInputThreshold)
        {
            ResetFootstepTimer();
            return;
        }

        if (footstepEvents == null || footstepEvents.Length == 0)
            return;

        footstepTimer += Time.deltaTime;

        if (footstepTimer >= nextFootstepInterval)
        {
            PlayRandomFootstep();
            footstepTimer = 0f;
            nextFootstepInterval = ComputeNextFootstepInterval();
        }
        else if (footstepTimer <= 0.1f)
        {
            PlayRandomFootstep();
            footstepTimer = 0.11f;
            // Debug.Log("FOOTSTEP first");
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepEvents.Length == 0) return;

        int idx = Random.Range(0, footstepEvents.Length);

        EventInstance ei = RuntimeManager.CreateInstance(footstepEvents[idx]);

        float randomPitch = Random.Range(minPitch, maxPitch);

        ei.setPitch(randomPitch);

        ei.setVolume(footstepVolume);

        ei.start();
        ei.release();
    }

    private float ComputeNextFootstepInterval()
    {
        float speedScale = Mathf.Clamp(Mathf.Abs(input), 0.25f, 1f);
        float interval = baseFootstepInterval / speedScale;

        float mul = Random.Range(footstepIntervalRandomMultiplier.x, footstepIntervalRandomMultiplier.y);
        return Mathf.Max(0.01f, interval * mul);
    }

    private void ResetFootstepTimer()
    {
        footstepTimer = 0f;
        nextFootstepInterval = baseFootstepInterval;
    }

    private void HandleFlip()
    {
        if (!spriteRenderer)
            return;

        if (input > 0)
            spriteRenderer.flipX = false;
        else if (input < 0)
            spriteRenderer.flipX = true;
    }

    private void SetIdleSprite()
    {
        if (!spriteRenderer) return;
        if (idleSprites == null || idleSprites.Length == 0) return;

        if (idleSprites[0])
            spriteRenderer.sprite = idleSprites[0];
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interactable = other.GetComponentInParent<Interactable>();
        if (!interactable)
            return;

        if (interactablesInRange.Add(interactable))
            interactable.OnEnterRange();

        UpdateCurrentInteractable();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.GetComponentInParent<Interactable>();
        if (!interactable)
            return;

        if (interactablesInRange.Remove(interactable))
            interactable.OnExitRange();

        if (currentInteractable == interactable)
            currentInteractable = null;

        UpdateCurrentInteractable();
    }

    private void UpdateCurrentInteractable()
    {
        CleanupInteractables();

        Interactable best = null;
        float bestDistSq = float.PositiveInfinity;
        Vector2 p = rb ? rb.position : (Vector2)transform.position;

        foreach (var i in interactablesInRange)
        {
            if (!i) continue;

            float d = (((Vector2)i.transform.position) - p).sqrMagnitude;
            if (d < bestDistSq)
            {
                bestDistSq = d;
                best = i;
            }
        }

        currentInteractable = best;

        if (currentInteractable != null && currentInteractable.UiAnchor != null)
            InteractionUI.Instance.Show(currentInteractable.UiAnchor);
        else
            InteractionUI.Instance.Hide();
    }

    private void ForceExitAllInteractables()
    {
        foreach (var i in interactablesInRange)
        {
            if (i) i.OnExitRange();
        }

        interactablesInRange.Clear();
        currentInteractable = null;

        if (InteractionUI.Instance != null)
            InteractionUI.Instance.Hide();
    }

    private void CleanupInteractables()
    {
        interactablesInRange.RemoveWhere(i =>
            i == null || !i.isActiveAndEnabled || !i.gameObject.activeInHierarchy);
    }
}
