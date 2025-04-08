using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls;

    private PlayerLocomotion playerLocomotion;
    private AnimatorManager animatorManager;

    // Input values from the Input System
    private Vector2 movementInput;
    private Vector2 cameraInput;
    private Vector2 rawGravityInput;

    // Processed input values
    private float moveAmount;
    private float verticalInput;
    private float horizontalInput;
    private float cameraInputX;
    private float cameraInputY;

    [SerializeField] private bool sprintInput;
    [SerializeField] private bool jumpInput;

    // Events for gravity preview/confirmation
    public event Action<Vector3> OnGravityConfirmed;
    public event Action<Vector3> OnGravityPreviewed;
    public event Action OnGravityPreviewCanceled;
    public event Action OnGravityPreviewConfirmed;

    private Vector3 lastPreviewDirection = Vector3.zero;
    private bool inputBlocked = false;

    private void Awake()
    {
        // Get references to related components
        animatorManager = GetComponentInChildren<AnimatorManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void OnEnable()
    {
        // Set up input actions
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            // Movement and camera
            playerControls.PlayerMovement.Movement.performed += Movement_Input_Performed;
            playerControls.PlayerMovement.Camera.performed += Camera_Input_Performed;

            // Sprint and jump
            playerControls.PlayerAction.Sprint.performed += Sprint_Input_Performed;
            playerControls.PlayerAction.Sprint.canceled += Sprint_Input_Canceled;
            playerControls.PlayerAction.Jump.performed += Jump_Input_Performed;

            // Gravity controls
            playerControls.PlayerAction.Gravity.performed += Gravity_Input_Performed;
            playerControls.PlayerAction.Gravity.canceled += Gravity_Input_Canceled;
            playerControls.PlayerAction.Confirm.performed += Gravity_Confirm_Performed;

            // Pause control
            playerControls.PlayerAction.Pause.performed += Pause_Performed;
        }

        playerControls.Enable();

        // Listen for game state changes
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        playerControls.Disable();

        // Unsubscribe from all input events
        playerControls.PlayerMovement.Movement.performed -= Movement_Input_Performed;
        playerControls.PlayerMovement.Camera.performed -= Camera_Input_Performed;
        playerControls.PlayerAction.Sprint.performed -= Sprint_Input_Performed;
        playerControls.PlayerAction.Sprint.canceled -= Sprint_Input_Canceled;
        playerControls.PlayerAction.Jump.performed -= Jump_Input_Performed;
        playerControls.PlayerAction.Gravity.performed -= Gravity_Input_Performed;
        playerControls.PlayerAction.Gravity.canceled -= Gravity_Input_Canceled;
        playerControls.PlayerAction.Confirm.performed -= Gravity_Confirm_Performed;
        playerControls.PlayerAction.Pause.performed -= Pause_Performed;

        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        // Force player orientation at start if no input yet
        if (movementInput == Vector2.zero)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 flatForward = new Vector3(camForward.x, 0f, camForward.z).normalized;
            Vector3 localForward = transform.InverseTransformDirection(flatForward);

            movementInput = new Vector2(localForward.x, localForward.z);
            HandleMovementInput();

            // Clear again to avoid unintended movement
            movementInput = Vector2.zero;
        }
    }

    // Input callbacks
    private void Jump_Input_Performed(InputAction.CallbackContext ctx) => jumpInput = true;
    private void Sprint_Input_Performed(InputAction.CallbackContext ctx) => sprintInput = true;
    private void Sprint_Input_Canceled(InputAction.CallbackContext ctx) => sprintInput = false;
    private void Camera_Input_Performed(InputAction.CallbackContext ctx) => cameraInput = ctx.ReadValue<Vector2>();
    private void Movement_Input_Performed(InputAction.CallbackContext ctx) => movementInput = ctx.ReadValue<Vector2>();
    private void Gravity_Input_Performed(InputAction.CallbackContext ctx) => rawGravityInput = ctx.ReadValue<Vector2>();
    private void Gravity_Input_Canceled(InputAction.CallbackContext ctx) => rawGravityInput = Vector2.zero;

    private void Gravity_Confirm_Performed(InputAction.CallbackContext ctx)
    {
        // Convert raw input into a world-aligned direction
        Vector2 snappedInput = GetSnappedGravityInput();
        if (snappedInput == Vector2.zero)
            return;

        Vector3 localDirection = new Vector3(snappedInput.x, 0f, snappedInput.y);
        Vector3 worldDirection = transform.TransformDirection(localDirection).normalized;
        Vector3 finalDirection = GetNearestWorldAxis(worldDirection);

        // Notify gravity change listeners
        OnGravityConfirmed?.Invoke(finalDirection);
        OnGravityPreviewConfirmed?.Invoke();
    }

    private void Pause_Performed(InputAction.CallbackContext ctx)
    {
        // Toggle pause/unpause if allowed
        if (GameManager.CurrentState == GameState.Playing || GameManager.CurrentState == GameState.Paused)
        {
            GameManager.Instance.TogglePause();
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        // Disable input if game is not in play state
        inputBlocked = state != GameState.Playing;
    }

    public void HandleAllInput()
    {
        if (inputBlocked)
            return;

        HandleMovementInput();
        HandleSprintingInput();
        HandleJumpingInput();
        HandleGravityPreviewInput();
    }

    // Public accessors
    public float GetVerticalInput() => verticalInput;
    public float GetHorizontalInput() => horizontalInput;
    public float GetCameraInputX() => cameraInputX;
    public float GetCameraInputY() => cameraInputY;
    public float GetMoveAmount() => moveAmount;
    public Vector2 GetGravityInput() => GetSnappedGravityInput();

    private void HandleMovementInput()
    {
        if (GameManager.CurrentState != GameState.Playing)
            return;

        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        cameraInputX = cameraInput.x;
        cameraInputY = cameraInput.y;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        animatorManager.UpdateAnimatorValues(0, moveAmount, playerLocomotion.GetIsSprinting());
    }

    private void HandleSprintingInput()
    {
        bool shouldSprint = sprintInput && moveAmount > 0.5f;
        playerLocomotion.SetIsSprinting(shouldSprint);
    }

    private void HandleJumpingInput()
    {
        if (jumpInput)
        {
            jumpInput = false;
            playerLocomotion.HandleJumping();
        }
    }

    private void HandleGravityPreviewInput()
    {
        Vector2 snappedInput = GetSnappedGravityInput();

        if (snappedInput != Vector2.zero)
        {
            Vector3 localDirection = new Vector3(snappedInput.x, 0f, snappedInput.y);
            Vector3 worldDirection = transform.TransformDirection(localDirection).normalized;
            Vector3 previewDirection = GetNearestWorldAxis(worldDirection);

            // Only send event if direction actually changed
            if (previewDirection != lastPreviewDirection)
            {
                lastPreviewDirection = previewDirection;
                OnGravityPreviewed?.Invoke(previewDirection);
            }

            Debug.DrawRay(transform.position, previewDirection * 2f, Color.cyan);
        }
        else
        {
            // Cancel preview if input is cleared
            if (lastPreviewDirection != Vector3.zero)
            {
                lastPreviewDirection = Vector3.zero;
                OnGravityPreviewCanceled?.Invoke();
            }
        }
    }

    // Converts raw input to one of four cardinal directions (snap)
    private Vector2 GetSnappedGravityInput()
    {
        float inputX = rawGravityInput.x;
        float inputY = rawGravityInput.y;

        bool isHorizontalDominant = Mathf.Abs(inputX) > Mathf.Abs(inputY);
        bool hasVerticalInput = Mathf.Abs(inputY) > 0f;

        if (isHorizontalDominant)
            return new Vector2(Mathf.Sign(inputX), 0f);
        else if (hasVerticalInput)
            return new Vector2(0f, Mathf.Sign(inputY));
        else
            return Vector2.zero;
    }

    // Determines closest world axis to a given direction
    private Vector3 GetNearestWorldAxis(Vector3 direction)
    {
        Vector3[] axes = {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            Vector3.up,
            Vector3.down
        };

        Vector3 nearestAxis = Vector3.zero;
        float maxAlignment = float.NegativeInfinity;

        foreach (Vector3 axis in axes)
        {
            float alignment = Vector3.Dot(direction, axis);
            if (alignment > maxAlignment)
            {
                maxAlignment = alignment;
                nearestAxis = axis;
            }
        }

        return nearestAxis;
    }
}
