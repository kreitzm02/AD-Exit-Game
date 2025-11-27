using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("MOVEMENT")]
    [SerializeField] private float moveSpeed = 3.0f;

    [Header("ANIMATION")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleAnimName = "IDLE";
    [SerializeField] private string walkAnimName = "WALK";
    [SerializeField] float animFadeTime = 0.1f;

    [Header("INPUT")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;

    private float input;
    private bool isMoving;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private Interactable currentInteractable;

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
        if (!animator)
            animator = GetComponentInChildren<Animator>();

        PlayIdle();
    }

    private void Update()
    {
        ReadInput();
        HandleAnimation();
        HandleFlip();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    public void LockInput(bool value)
    {
        InputLocked = value;
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

    private void HandleAnimation()
    {
        if (isMoving)
            PlayWalk();
        else
            PlayIdle();
    }

    private void HandleFlip()
    {
        if (input > 0)
            spriteRenderer.flipX = false;
        else if (input < 0)
            spriteRenderer.flipX = true;
    }

    private void PlayIdle()
    {
        animator.CrossFade(idleAnimName, animFadeTime);
    }

    private void PlayWalk()
    {
        animator.CrossFade(walkAnimName, animFadeTime);
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
