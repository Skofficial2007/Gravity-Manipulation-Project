using System;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    // Event triggered when the player falls for a significant amount of time
    public static event Action OnPlayerFell;

    // References to required components
    private PlayerManager playerManager;
    private InputManager inputManager;
    private AnimatorManager animatorManager;
    private Transform cameraObject;
    private Rigidbody playerRigidbody;

    // Direction the player should move in
    private Vector3 moveDirection;

    // Cached input values
    private float verticalInput;
    private float horizontalInput;

    [Header("Falling Settings")]
    [SerializeField] private float inAirTimer = 0f; // Tracks how long the player has been falling
    [SerializeField] private float leapingVelocity = 2f; // Forward force while in air
    [SerializeField] private float fallingVelocity = 30f; // Gravity multiplier during fall
    [SerializeField] private float rayCastHeightOffset = 0.5f; // Distance above feet to cast for ground
    [SerializeField] private LayerMask groundLayer; // Layer used to detect ground
    [SerializeField] private float fallThresholdTime = 2f; // Time before fall is considered significant

    [Header("Movement Flags")]
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isJumping;

    [Header("Movement Speeds")]
    [SerializeField] private float walkingSpeed = 1.5f;
    [SerializeField] private float runningSpeed = 5f;
    [SerializeField] private float sprintingSpeed = 8f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravityIntensity = -9f;

    private bool hasTriggeredFallEvent = false; // Prevents repeated event calls

    private void Awake()
    {
        // Cache references to other components
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        animatorManager = GetComponentInChildren<AnimatorManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    // Public getters and setters for key flags
    public bool GetIsSprinting() => isSprinting;
    public void SetIsSprinting(bool val) => isSprinting = val;
    public bool GetIsJumping() => isJumping;
    public void SetIsJumping(bool val) => isJumping = val;
    public bool GetIsGrounded() => isGrounded;

    // Main method to be called every frame for movement handling
    public void HandleAllMovement()
    {
        // Do nothing if game isn't in play mode
        if (GameManager.CurrentState != GameState.Playing)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            return;
        }

        // Check for fall and land transitions
        HandleFallingAndLanding();

        // Don't move if performing other interactions
        if (playerManager.GetIsInteracting()) return;

        // Get fresh input
        verticalInput = inputManager.GetVerticalInput();
        horizontalInput = inputManager.GetHorizontalInput();

        // Apply movement and rotation
        HandleMovement();
        HandleRotation();
    }

    // Handles grounded movement logic
    private void HandleMovement()
    {
        if (isJumping || playerManager.GetIsInteracting()) return;

        // Use camera orientation relative to gravity for movement direction
        Vector3 gravityDir = Physics.gravity.normalized;
        Vector3 camForward = Vector3.ProjectOnPlane(cameraObject.forward, gravityDir).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraObject.right, gravityDir).normalized;

        moveDirection = camForward * verticalInput + camRight * horizontalInput;
        moveDirection.Normalize();

        // Choose speed based on input and sprint flag
        float speed = isSprinting ? sprintingSpeed :
                     (inputManager.GetMoveAmount() >= 0.5f ? runningSpeed : walkingSpeed);

        moveDirection *= speed;

        // Apply movement only if grounded
        if (isGrounded && !isJumping)
        {
            playerRigidbody.linearVelocity = moveDirection;
        }
    }

    // Handles rotating the player to match movement direction
    private void HandleRotation()
    {
        if (isJumping || playerManager.GetIsInteracting()) return;

        Vector3 gravityDir = Physics.gravity.normalized;
        Vector3 camForward = Vector3.ProjectOnPlane(cameraObject.forward, gravityDir).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraObject.right, gravityDir).normalized;

        Vector3 targetDirection = camForward * verticalInput + camRight * horizontalInput;
        targetDirection.Normalize();

        if (targetDirection == Vector3.zero)
            return;

        // Rotate the player to face the direction of movement
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, -gravityDir);
        Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (isGrounded && !isJumping)
        {
            playerRigidbody.MoveRotation(smoothRotation);
        }
    }

    // Handles the logic for falling and landing
    private void HandleFallingAndLanding()
    {
        Vector3 gravityDir = Physics.gravity.normalized;
        Vector3 rayOrigin = transform.position - gravityDir * rayCastHeightOffset;

        // If airborne and not jumping, play falling animation and apply forces
        if (!isGrounded && !isJumping)
        {
            if (!playerManager.GetIsInteracting())
            {
                animatorManager.PlayTargetAnimations("Falling", true);
                playerManager.SetIsInteracting(true);
            }

            inAirTimer += Time.deltaTime;

            // Apply forward momentum and increased falling force
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            playerRigidbody.AddForce(gravityDir * fallingVelocity * inAirTimer);

            // If falling for long enough, trigger the fall event
            if (inAirTimer > fallThresholdTime && !hasTriggeredFallEvent)
            {
                hasTriggeredFallEvent = true;
                OnPlayerFell?.Invoke();
            }
        }

        // Cast a sphere downwards to check for ground
        float maxDistance = 0.5f;
        float sphereRadius = 0.2f;

        if (Physics.SphereCast(rayOrigin, sphereRadius, gravityDir, out RaycastHit hit, maxDistance, groundLayer))
        {
            // If landing from a fall, play landing animation
            if (!isGrounded && playerManager.GetIsInteracting())
            {
                animatorManager.PlayTargetAnimations("Landing", true);
            }

            isGrounded = true;
            inAirTimer = 0f;
            hasTriggeredFallEvent = false;
        }
        else
        {
            isGrounded = false;
        }
    }

    // Handles jumping logic
    public void HandleJumping()
    {
        // Prevent jumping if in air or mid-action
        if (!isGrounded || playerManager.GetIsInteracting()) return;

        // Trigger jump animation
        animatorManager.animator.SetBool("isJumping", true);
        animatorManager.PlayTargetAnimations("Jump", false);

        // Calculate upward jump force
        float jumpVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
        Vector3 jumpForce = -Physics.gravity.normalized * jumpVelocity;

        // Add a forward boost and combine with jump force
        Vector3 forwardBoost = moveDirection.normalized * leapingVelocity;
        Vector3 finalVelocity = forwardBoost + jumpForce;

        // Apply the final velocity to the rigidbody
        playerRigidbody.linearVelocity = finalVelocity;
    }
}
