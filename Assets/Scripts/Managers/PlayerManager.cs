using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //[SerializeField] private CameraManager cameraManager; // Optional camera system (disabled)

    private Animator animator;                       // Handles character animations (jumping, interacting, etc.)
    private InputManager inputManager;               // Processes input from player controls
    private PlayerLocomotion playerLocomotion;       // Controls movement and physics behavior

    private bool isInteracting;                      // Tracks whether the player is in an interaction animation

    private void Awake()
    {
        // Cache component references from this GameObject or children
        animator = GetComponentInChildren<Animator>();
        inputManager = GetComponent<InputManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void Update()
    {
        inputManager.HandleAllInput();
    }

    private void FixedUpdate()
    {
        // Ensures smoother and more stable movement using Rigidbody
        playerLocomotion.HandleAllMovement();
    }

    private void LateUpdate()
    {
        // Ensures camera updates happen after player has moved
        // cameraManager.HandleAllCameraMovement(); // Optional external camera handler

        // Reads the "isInteracting" flag from Animator
        // Used to block movement during animations
        isInteracting = animator.GetBool("isInteracting");

        // Keeps the locomotion logic aware of jumping animation state
        // Allows logic to detect when the jump ends and apply landing
        playerLocomotion.SetIsJumping(animator.GetBool("isJumping"));

        // Keeps Animator grounded state in sync with actual physics grounding
        // Typically drives grounded/airborne blend trees
        animator.SetBool("isGrounded", playerLocomotion.GetIsGrounded());
    }

    // Returns whether the character is currently interacting (e.g., can't move)
    public bool GetIsInteracting() => isInteracting;

    // Allows external scripts (like animation events) to change interaction state
    public void SetIsInteracting(bool val) => isInteracting = val;
}
