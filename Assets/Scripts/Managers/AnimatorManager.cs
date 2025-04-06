using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    [Header("Animator Reference")]
    public Animator animator;

    // Parameter hashes for blend tree control
    private int horizontal;
    private int vertical;

    // Flag to prevent animation updates when game isn't playing
    private bool animationBlocked = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        // Cache animator parameter hashes for performance
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    // Blocks or unblocks animations based on game state
    private void HandleGameStateChanged(GameState state)
    {
        animationBlocked = state != GameState.Playing;

        if (animationBlocked)
        {
            // Stop movement animations immediately
            animator.SetFloat(horizontal, 0);
            animator.SetFloat(vertical, 0);
            animator.SetBool("isInteracting", false);

            // Optional: force to Idle animation if available
            if (animator.HasState(0, Animator.StringToHash("Idle")))
            {
                animator.CrossFade("Idle", 0.1f);
            }
        }
    }

    // Plays a target animation with optional interaction lock
    public void PlayTargetAnimations(string targetAnimation, bool isInteracting)
    {
        animator.SetBool("isInteracting", isInteracting);
        animator.CrossFade(targetAnimation, 0.2f);
    }

    // Updates movement parameters for blend tree animation
    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement, bool isSprinting)
    {
        if (animationBlocked) return;

        float snappedHorizontal = GetSnappedValue(horizontalMovement);
        float snappedVertical = GetSnappedValue(verticalMovement);

        // Sprint override logic
        if (isSprinting)
        {
            snappedHorizontal = horizontalMovement;
            snappedVertical = 2; // custom sprint value in blend tree
        }

        animator.SetFloat(horizontal, snappedHorizontal, 0.1f, Time.deltaTime);
        animator.SetFloat(vertical, snappedVertical, 0.1f, Time.deltaTime);
    }

    // Converts float input into stepped values for cleaner animation transitions
    private float GetSnappedValue(float movement)
    {
        float snappedValue;

        // Snap input values into ranges: -1, -0.5, 0, 0.5, or 1
        // This helps trigger precise animation states in a blend tree
        if (movement > 0 && movement < 0.55f)
        {
            snappedValue = 0.5f;
        }
        else if (movement > 0.55f)
        {
            snappedValue = 1f;
        }
        else if (movement < 0 && movement > -0.55f)
        {
            snappedValue = -0.5f;
        }
        else if (movement < -0.55f)
        {
            snappedValue = -1f;
        }
        else
        {
            snappedValue = 0;
        }

        return snappedValue;
    }
}
