using UnityEngine;

public class GravityManager : MonoBehaviour
{
    [Header("Dependencies")]
    //[SerializeField] private CameraManager cameraManager; // Optional: for aligning camera orientation to gravity
    [SerializeField] private LayerMask groundLayer;           // Layer used to detect ground surfaces beneath player

    [Header("Gravity Settings")]
    [SerializeField] private float gravityStrength = 9.81f;   // Force magnitude applied in gravity direction

    private InputManager inputManager;                        // Reference to InputManager for gravity input events

    // Represents the current gravity direction in world space (shared globally)
    public static Vector3 CurrentGravity { get; private set; } = Vector3.down;

    private void Awake()
    {
        // Grab InputManager from the same GameObject (assumes both components are attached)
        inputManager = GetComponent<InputManager>();

        // Reset static gravity and apply default force downward
        CurrentGravity = Vector3.down;
        Physics.gravity = CurrentGravity * gravityStrength;

        // Align the player’s orientation with gravity when the scene loads
        AlignPlayerToGravity(CurrentGravity);
    }

    private void OnEnable()
    {
        // Register listener for confirmed gravity direction from player input
        inputManager.OnGravityConfirmed += HandleGravityChange;
    }

    private void OnDisable()
    {
        // Unregister to prevent unwanted calls or memory issues
        inputManager.OnGravityConfirmed -= HandleGravityChange;
    }

    // Called when the player confirms a new gravity direction
    private void HandleGravityChange(Vector3 gravityDirection)
    {
        // Update global gravity reference
        CurrentGravity = gravityDirection;

        // Set the Unity physics system's gravity to the new direction
        Physics.gravity = gravityDirection * gravityStrength;

        // Reorient the player so their feet point opposite gravity
        AlignPlayerToGravity(gravityDirection);

        // Optionally snap player to the ground in the new gravity direction
        SnapPlayerToNewGround(gravityDirection);

        // Align the camera to maintain upright feel (if camera system supports it)
        //cameraManager.AlignToGravity(transform);
    }

    // Rotates the player so that their 'up' aligns against gravity direction
    private void AlignPlayerToGravity(Vector3 gravityDirection)
    {
        // Compute the rotation needed to make player's up match the opposite of gravity
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = targetRotation;
    }

    // Attempts to reposition the player onto the surface aligned with new gravity
    private void SnapPlayerToNewGround(Vector3 gravityDirection)
    {
        Vector3 rayOrigin = transform.position;
        RaycastHit hit;

        // Cast a ray in the gravity direction to find the nearest surface
        if (Physics.Raycast(rayOrigin, gravityDirection, out hit, 10f, groundLayer))
        {
            // Move player just above the detected surface (small offset to avoid clipping)
            Vector3 snappedPosition = hit.point - gravityDirection.normalized * 0.5f;
            transform.position = snappedPosition;
        }

        // If no ground is found, player will fall — handled by GameManager or death logic
    }

    // Fully resets gravity to default downward orientation
    public void ResetGravityToDefault()
    {
        CurrentGravity = Vector3.down;
        Physics.gravity = CurrentGravity * gravityStrength;

        // Reorient and reposition player to match default gravity
        AlignPlayerToGravity(CurrentGravity);
        SnapPlayerToNewGround(CurrentGravity);
    }
}
