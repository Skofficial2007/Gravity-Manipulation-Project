using System;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    // Event to notify when the player falls for too long (e.g. falls off the map)
    public static event Action OnPlayerFell;

    // Component references
    private PlayerManager playerManager;
    private InputManager inputManager;
    private AnimatorManager animatorManager;
    private Transform cameraObject;
    private Rigidbody playerRigidbody;

    // Direction to move the player
    private Vector3 moveDirection;

    // Input values
    private float verticalInput;
    private float horizontalInput;

    [Header("Falling Settings")]
    [SerializeField] private float inAirTimer = 0f;
    [SerializeField] private float leapingVelocity = 2f;
    [SerializeField] private float fallingVelocity = 30f;
    [SerializeField] private float rayCastHeightOffset = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float fallThresholdTime = 2f;

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

    // Prevent triggering the fall event multiple times
    private bool hasTriggeredFallEvent = false;

    private void Awake()
    {
        // Cache component references on awake
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        animatorManager = GetComponentInChildren<AnimatorManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    // Accessors for state flags
    public bool GetIsSprinting() => isSprinting;
    public void SetIsSprinting(bool val) => isSprinting = val;
    public bool GetIsJumping() => isJumping;
    public void SetIsJumping(bool val) => isJumping = val;
    public bool GetIsGrounded() => isGrounded;

    public void HandleAllMovement()
    {
        // Stop all movement when game is not playing
        if (GameManager.CurrentState != GameState.Playing)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            return;
        }

        // Handle falling first to update grounded state
        HandleFallingAndLanding();

        // Skip movement if in animation interaction
        if (playerManager.GetIsInteracting()) return;

        // Get player input
        verticalInput = inputManager.GetVerticalInput();
        horizontalInput = inputManager.GetHorizontalInput();

        // Move and rotate based on input
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        // Do not move while in the middle of a jump
        if (isJumping) return;

        Vector3 gravityDir = Physics.gravity.normalized;

        // Camera-relative movement directions
        Vector3 camForward = Vector3.ProjectOnPlane(cameraObject.forward, gravityDir).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraObject.right, gravityDir).normalized;

        moveDirection = camForward * verticalInput + camRight * horizontalInput;
        moveDirection.Normalize();

        // Determine speed based on input magnitude and sprinting state
        float speed = isSprinting ? sprintingSpeed :
                     (inputManager.GetMoveAmount() >= 0.5f ? runningSpeed : walkingSpeed);

        moveDirection *= speed;

        // Apply movement only if grounded and not jumping
        if (isGrounded && !isJumping)
        {
            playerRigidbody.linearVelocity = moveDirection;
        }
    }

    private void HandleRotation()
    {
        // Do not rotate during a jump
        if (isJumping) return;

        Vector3 gravityDir = Physics.gravity.normalized;

        // Get input direction relative to camera
        Vector3 camForward = Vector3.ProjectOnPlane(cameraObject.forward, gravityDir).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraObject.right, gravityDir).normalized;

        Vector3 targetDirection = camForward * verticalInput + camRight * horizontalInput;
        targetDirection.Normalize();

        // Maintain current facing if no input
        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        // Smoothly rotate towards movement direction
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, -gravityDir);
        Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (isGrounded && !isJumping)
        {
            transform.rotation = smoothRotation;
        }
    }

    private void HandleFallingAndLanding()
    {
        Vector3 gravityDir = Physics.gravity.normalized;
        Vector3 rayOrigin = transform.position - gravityDir * rayCastHeightOffset;

        // If in the air and not jumping, play falling behavior
        if (!isGrounded && !isJumping)
        {
            if (!playerManager.GetIsInteracting())
                animatorManager.PlayTargetAnimations("Falling", true);

            inAirTimer += Time.deltaTime;

            // Add forward momentum and downward force
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            playerRigidbody.AddForce(gravityDir * fallingVelocity * inAirTimer);

            // Trigger fall event if in air too long
            if (inAirTimer > fallThresholdTime && !hasTriggeredFallEvent)
            {
                hasTriggeredFallEvent = true;
                OnPlayerFell?.Invoke();
            }
        }

        // Use sphere cast to detect ground below
        float maxDistance = 0.5f;
        float sphereRadius = 0.2f;

        if (Physics.SphereCast(rayOrigin, sphereRadius, gravityDir, out RaycastHit hit, maxDistance, groundLayer))
        {
            // If landing after interaction, play landing animation
            if (!isGrounded && playerManager.GetIsInteracting())
            {
                animatorManager.PlayTargetAnimations("Landing", true);
            }

            isGrounded = true;
            inAirTimer = 0f;
            hasTriggeredFallEvent = false;
            playerManager.SetIsInteracting(false);
        }
        else
        {
            isGrounded = false;
        }
    }

    public void HandleJumping()
    {
        // Only jump if grounded
        if (!isGrounded) return;

        // Play jump animation
        animatorManager.animator.SetBool("isJumping", true);
        animatorManager.PlayTargetAnimations("Jump", false);

        // Calculate initial jump velocity using physics formula
        float jumpVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
        Vector3 jumpForce = -Physics.gravity.normalized * jumpVelocity;

        // Apply jump along with current movement direction
        Vector3 finalVelocity = moveDirection + jumpForce;
        playerRigidbody.linearVelocity = finalVelocity;
    }
}
