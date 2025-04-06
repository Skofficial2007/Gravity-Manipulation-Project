using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls;

    private PlayerLocomotion playerLocomotion;         // Handles actual physical movement of the player
    private AnimatorManager animatorManager;           // Responsible for updating Animator parameters

    // Caches raw and processed input values each frame.
    private Vector2 movementInput;                     // Stores directional input for movement (WASD / left stick)
    private Vector2 cameraInput;                       // Stores camera control input (mouse or right stick)
    private Vector2 rawGravityInput;                   // Raw input for gravity direction (before snapping)

    // Used to break down movement and camera inputs into separate axes.
    private float moveAmount;                          // Combined magnitude of input (used to drive blend tree values)
    private float verticalInput;                       // Forward/backward movement input
    private float horizontalInput;                     // Left/right movement input
    private float cameraInputX;                        // Horizontal look input
    private float cameraInputY;                        // Vertical look input

    // Input flags (set through callbacks, used during logic update)
    [SerializeField] private bool sprintInput;
    [SerializeField] private bool jumpInput;

    // Events triggered during gravity shift interactions (previewing or confirming)
    public event Action<Vector3> OnGravityConfirmed;
    public event Action<Vector3> OnGravityPreviewed;
    public event Action OnGravityPreviewCanceled;
    public event Action OnGravityPreviewConfirmed;

    // Tracks last preview direction to avoid redundant event calls
    private Vector3 lastPreviewDirection = Vector3.zero;

    // Whether input should be ignored due to game state
    private bool inputBlocked = false;

    private void Awake()
    {
        animatorManager = GetComponentInChildren<AnimatorManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    // Input System setup and event registration
    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            // Movement and camera bindings
            playerControls.PlayerMovement.Movement.performed += Movement_Input_Performed;
            playerControls.PlayerMovement.Camera.performed += Camera_Input_Performed;

            // Sprint and jump inputs
            playerControls.PlayerAction.Sprint.performed += Sprint_Input_Performed;
            playerControls.PlayerAction.Sprint.canceled += Sprint_Input_Canceled;
            playerControls.PlayerAction.Jump.performed += Jump_Input_Performed;

            // Gravity selection input
            playerControls.PlayerAction.Gravity.performed += Gravity_Input_Performed;
            playerControls.PlayerAction.Gravity.canceled += Gravity_Input_Canceled;
            playerControls.PlayerAction.Confirm.performed += Gravity_Confirm_Performed;

            // Pause menu toggle
            playerControls.PlayerAction.Pause.performed += Pause_Performed;
        }

        // Enable the input map
        playerControls.Enable();

        // Listen for game state changes to enable/disable input
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    // Unregister all input callbacks and event listeners when disabled
    private void OnDisable()
    {
        playerControls.Disable();

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

    // Input callbacks update their respective input flags or vectors

    private void Jump_Input_Performed(InputAction.CallbackContext ctx)
    {
        jumpInput = true;
    }

    private void Sprint_Input_Performed(InputAction.CallbackContext ctx)
    {
        sprintInput = true;
    }

    private void Sprint_Input_Canceled(InputAction.CallbackContext ctx)
    {
        sprintInput = false;
    }

    private void Camera_Input_Performed(InputAction.CallbackContext ctx)
    {
        cameraInput = ctx.ReadValue<Vector2>();
    }

    private void Movement_Input_Performed(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }

    private void Gravity_Input_Performed(InputAction.CallbackContext ctx)
    {
        rawGravityInput = ctx.ReadValue<Vector2>();
    }

    private void Gravity_Input_Canceled(InputAction.CallbackContext ctx)
    {
        rawGravityInput = Vector2.zero;
    }

    // Called when player confirms a gravity direction (e.g., pressing confirm button)
    private void Gravity_Confirm_Performed(InputAction.CallbackContext ctx)
    {
        Vector2 snappedInput = GetSnappedGravityInput();

        if (snappedInput == Vector2.zero)
            return;

        // Convert local input direction to world space and find nearest axis (e.g., Up, Right)
        Vector3 localDirection = new Vector3(snappedInput.x, 0f, snappedInput.y);
        Vector3 worldDirection = transform.TransformDirection(localDirection).normalized;
        Vector3 finalDirection = GetNearestWorldAxis(worldDirection);

        // Notify any subscribers about confirmed gravity direction
        OnGravityConfirmed?.Invoke(finalDirection);
        OnGravityPreviewConfirmed?.Invoke();
    }

    private void Pause_Performed(InputAction.CallbackContext ctx)
    {
        // Toggles pause state only if in valid states
        if (GameManager.CurrentState == GameState.Playing || GameManager.CurrentState == GameState.Paused)
        {
            GameManager.Instance.TogglePause();
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        // Prevents input handling when the game is not actively playing
        inputBlocked = state != GameState.Playing;
    }

    // Called from an external MonoBehaviour (e.g., PlayerManager) every frame
    public void HandleAllInput()
    {
        if (inputBlocked)
            return;

        HandleMovementInput();
        HandleSprintingInput();
        HandleJumpingInput();
        HandleGravityPreviewInput();
    }

    // These are public getters, often used by external systems (camera, movement)
    public float GetVerticalInput() => verticalInput;
    public float GetHorizontalInput() => horizontalInput;
    public float GetCameraInputX() => cameraInputX;
    public float GetCameraInputY() => cameraInputY;
    public float GetMoveAmount() => moveAmount;
    public Vector2 GetGravityInput() => GetSnappedGravityInput();

    // Processes current frame's movement input and sends movement data to animator
    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        cameraInputX = cameraInput.x;
        cameraInputY = cameraInput.y;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        // Update animator with movement magnitude and sprinting state
        animatorManager.UpdateAnimatorValues(0, moveAmount, playerLocomotion.GetIsSprinting());
    }

    // Enables sprinting based on input and movement strength
    private void HandleSprintingInput()
    {
        bool shouldSprint = sprintInput && moveAmount > 0.5f;
        playerLocomotion.SetIsSprinting(shouldSprint);
    }

    // Triggers the jump logic once per press
    private void HandleJumpingInput()
    {
        if (jumpInput)
        {
            jumpInput = false;
            playerLocomotion.HandleJumping();
        }
    }

    // Continuously processes gravity direction preview input
    private void HandleGravityPreviewInput()
    {
        Vector2 snappedInput = GetSnappedGravityInput();

        if (snappedInput != Vector2.zero)
        {
            Vector3 localDirection = new Vector3(snappedInput.x, 0f, snappedInput.y);
            Vector3 worldDirection = transform.TransformDirection(localDirection).normalized;
            Vector3 previewDirection = GetNearestWorldAxis(worldDirection);

            // Only invoke if the direction actually changed from the last preview
            if (previewDirection != lastPreviewDirection)
            {
                lastPreviewDirection = previewDirection;
                OnGravityPreviewed?.Invoke(previewDirection);
            }

            // Helpful visual debug line in Scene view
            Debug.DrawRay(transform.position, previewDirection * 2f, Color.cyan);
        }
        else
        {
            // If there was a previous preview direction, clear it and notify listeners
            if (lastPreviewDirection != Vector3.zero)
            {
                lastPreviewDirection = Vector3.zero;
                OnGravityPreviewCanceled?.Invoke();
            }
        }
    }

    // Processes directional gravity input and snaps it to a single axis (left, right, up, down)
    private Vector2 GetSnappedGravityInput()
    {
        float inputX = rawGravityInput.x;
        float inputY = rawGravityInput.y;

        bool isHorizontalDominant = Mathf.Abs(inputX) > Mathf.Abs(inputY);
        bool hasVerticalInput = Mathf.Abs(inputY) > 0f;

        if (isHorizontalDominant)
        {
            // Snap gravity to horizontal axis (left/right)
            return new Vector2(Mathf.Sign(inputX), 0f);
        }
        else if (hasVerticalInput)
        {
            // Snap gravity to vertical axis (forward/backward)
            return new Vector2(0f, Mathf.Sign(inputY));
        }
        else
        {
            // No valid gravity direction input
            return Vector2.zero;
        }
    }

    // Given a direction vector, returns the nearest world axis (6 total possibilities)
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
