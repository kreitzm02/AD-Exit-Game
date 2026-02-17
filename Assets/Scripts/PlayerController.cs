using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("MOVEMENT")]
    [SerializeField] private float moveSpeed = 3.0f;

    [Header("ANIMATION")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField, Min(1f)] private float walkFps = 10f;

    [Header("FOOTSTEPS")]
    [SerializeField] private EventReference[] footstepEvents;
    [SerializeField, Min(0.01f)] private float baseFootstepInterval = 0.35f;
    [SerializeField] private Vector2 footstepIntervalRandomMultiplier = new Vector2(0.9f, 1.1f);
    [SerializeField, Range(0f, 1f)] private float footstepInputThreshold = 0.1f;
    [SerializeField, Min(0.01f)] private float minPitch = 0.9f;
    [SerializeField, Min(0.01f)] private float maxPitch = 1.1f;

    [Header("INPUT")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;

    private float input;
    private bool isMoving;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private Interactable currentInteractable;

    private int walkFrameIndex;
    private float walkFrameTimer;

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
    }

    private void Start()
    {
        if (!idleSprite && spriteRenderer)
            idleSprite = spriteRenderer.sprite;

        SetIdleSprite();
        ResetFootstepTimer();
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
            SetIdleSprite();
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
        if (!spriteRenderer)
            return;

        if (!isMoving || walkSprites == null || walkSprites.Length == 0)
        {
            SetIdleSprite();
            walkFrameTimer = 0f;
            walkFrameIndex = 0;
            return;
        }

        walkFrameTimer += Time.deltaTime;
        float frameDuration = 1f / walkFps;

        while (walkFrameTimer >= frameDuration)
        {
            walkFrameTimer -= frameDuration;
            walkFrameIndex = (walkFrameIndex + 1) % walkSprites.Length;
        }

        Sprite s = walkSprites[walkFrameIndex];
        if (s)
            spriteRenderer.sprite = s;
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
            Debug.Log("FOOTSTEP AFTER INTERVALL");
        }
        else if (footstepTimer <= 0.1f)
        {
            PlayRandomFootstep();
            footstepTimer = 0.11f;
            Debug.Log("FOOTSTEP first");
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepEvents.Length == 0) return;

        int idx = Random.Range(0, footstepEvents.Length);

        EventInstance ei = RuntimeManager.CreateInstance(footstepEvents[idx]);

        float randomPitch = Random.Range(minPitch, maxPitch);

        ei.setPitch(randomPitch);

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
        if (spriteRenderer && idleSprite)
            spriteRenderer.sprite = idleSprite;
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
        Interactable interactable = other.GetComponent<Interactable>();
        if (!interactable)
            return;

        currentInteractable = interactable;
        interactable.OnEnterRange();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (currentInteractable == null)
            return;

        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable == currentInteractable)
        {
            interactable.OnExitRange();
            currentInteractable = null;
        }
    }
}
