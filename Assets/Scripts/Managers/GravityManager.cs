using UnityEngine;

public class GravityManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityStrength = 9.81f;

    private InputManager inputManager;
    private Rigidbody playerRigidbody;

    // Stores the current gravity direction for global access
    public static Vector3 CurrentGravity { get; private set; } = Vector3.down;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();

        // Initialize default gravity pointing down
        CurrentGravity = Vector3.down;
        Physics.gravity = CurrentGravity * gravityStrength;

        // Ensure player starts aligned to initial gravity
        AlignPlayerToGravity(CurrentGravity);
    }

    private void OnEnable()
    {
        // Subscribe to gravity change input
        inputManager.OnGravityConfirmed += HandleGravityChange;
    }

    private void OnDisable()
    {
        inputManager.OnGravityConfirmed -= HandleGravityChange;
    }

    private void HandleGravityChange(Vector3 gravityDirection)
    {
        // Apply new gravity direction and strength
        CurrentGravity = gravityDirection;
        Physics.gravity = gravityDirection * gravityStrength;

        // Align and reposition player to match new gravity
        AlignPlayerToGravity(gravityDirection);
        SnapPlayerToNewGround(gravityDirection);
    }

    // Rotates the player so their 'up' is opposite of gravity
    private void AlignPlayerToGravity(Vector3 gravityDirection)
    {
        Vector3 gravityUp = -gravityDirection.normalized;

        // Project current forward onto new ground plane
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, gravityUp).normalized;

        // Use fallback if forward becomes degenerate
        if (forward == Vector3.zero)
            forward = Vector3.ProjectOnPlane(Vector3.forward, gravityUp);

        Quaternion targetRotation = Quaternion.LookRotation(forward, gravityUp);

        // Use Rigidbody's rotation when available to keep physics stable
        if (playerRigidbody != null)
        {
            playerRigidbody.MoveRotation(targetRotation);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    // Casts ray in new gravity direction and moves player to the surface
    private void SnapPlayerToNewGround(Vector3 gravityDirection)
    {
        Vector3 rayOrigin = transform.position;
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, gravityDirection, out hit, 10f, groundLayer))
        {
            // Offset slightly above ground to prevent embedding
            Vector3 snappedPosition = hit.point - gravityDirection.normalized * 0.5f;
            transform.position = snappedPosition;

            // Ensure rotation is reapplied after repositioning
            AlignPlayerToGravity(gravityDirection);
        }
    }

    // Resets to default downward gravity
    public void ResetGravityToDefault()
    {
        CurrentGravity = Vector3.down;
        Physics.gravity = CurrentGravity * gravityStrength;

        AlignPlayerToGravity(CurrentGravity);
        SnapPlayerToNewGround(CurrentGravity);
    }
}
