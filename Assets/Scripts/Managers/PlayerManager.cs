using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private Animator animator;
    private InputManager inputManager;
    private PlayerLocomotion playerLocomotion;

    // Tracks whether the player is currently in an animation that locks movement
    private bool isInteracting;

    private void Awake()
    {
        // Cache required component references
        animator = GetComponentInChildren<Animator>();
        inputManager = GetComponent<InputManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void Start()
    {
        // Rotate the player to face the camera's forward direction at the beginning
        InitializeDefaultForward();
    }

    private void Update()
    {
        // Handle all input per frame (movement, gravity, jumping)
        inputManager.HandleAllInput();
    }

    private void FixedUpdate()
    {
        // Perform physics-based movement logic
        playerLocomotion.HandleAllMovement();
    }

    private void LateUpdate()
    {
        // Sync internal state from Animator after movement and input

        // Used to prevent movement or input during certain animations (e.g. landing)
        isInteracting = animator.GetBool("isInteracting");

        // Notify locomotion when jump animation is active
        playerLocomotion.SetIsJumping(animator.GetBool("isJumping"));

        // Keep animator grounded state in sync with actual ground detection
        animator.SetBool("isGrounded", playerLocomotion.GetIsGrounded());
    }

    // Expose interaction flag to other systems (like locomotion)
    public bool GetIsInteracting() => isInteracting;

    // Allow external scripts or animation events to set interaction flag
    public void SetIsInteracting(bool val) => isInteracting = val;

    // Aligns player facing direction to camera forward (on horizontal plane)
    private void InitializeDefaultForward()
    {
        Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;

        if (camForward != Vector3.zero)
        {
            transform.forward = camForward;
        }
    }
}
