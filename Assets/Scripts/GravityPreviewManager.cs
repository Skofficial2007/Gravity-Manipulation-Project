using UnityEngine;

public class GravityPreviewManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputManager input;                   // Reference to the input system for gravity events
    [SerializeField] private Transform playerTransform;            // Player's transform to calculate hologram orientation
    [SerializeField] private GameObject currentHologram;           // The visual preview object shown during gravity selection

    [Header("Settings")]
    [SerializeField] private float gravityOffsetDistance = 3.5f;   // How far from the player the hologram is placed (in gravity direction)
    [SerializeField] private float liftOffset = 2f;                // How far to lift the hologram above the ground (along current up)

    private Quaternion previewRotation;                            // Cached rotation used when confirming gravity direction
    private Vector3 previewGravityDirection;                       // Cached direction used to place and rotate the hologram

    private void OnEnable()
    {
        if (input == null)
        {
            Debug.LogError("GravityPreviewManager: InputManager not assigned!");
            return;
        }

        // Subscribe to gravity-related preview events from input system
        input.OnGravityPreviewed += ShowPreview;
        input.OnGravityPreviewCanceled += HidePreview;
        input.OnGravityPreviewConfirmed += ApplyHologramRotation;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks or dangling references
        if (input == null) return;

        input.OnGravityPreviewed -= ShowPreview;
        input.OnGravityPreviewCanceled -= HidePreview;
        input.OnGravityPreviewConfirmed -= ApplyHologramRotation;
    }

    // Called when a gravity direction is being hovered (previewed) by the player
    private void ShowPreview(Vector3 gravityDirection)
    {
        if (playerTransform == null || currentHologram == null)
            return;

        previewGravityDirection = gravityDirection;

        // Determine the new "up" based on opposite of gravity
        Vector3 gravityUp = -gravityDirection.normalized;

        // Project current forward vector onto the new up plane to avoid odd angles
        Vector3 playerForward = Vector3.ProjectOnPlane(playerTransform.forward, gravityUp).normalized;

        // Fallback to original forward if projection fails (e.g., perfectly aligned)
        if (playerForward == Vector3.zero)
            playerForward = playerTransform.forward;

        // Build rotation facing forward, aligned to new up
        previewRotation = Quaternion.LookRotation(playerForward, gravityUp);

        // Position hologram ahead in gravity direction, offset by lift to avoid clipping
        Vector3 basePosition = playerTransform.position + gravityDirection.normalized * gravityOffsetDistance;
        Vector3 liftedPosition = basePosition + (-GravityManager.CurrentGravity.normalized * liftOffset);

        currentHologram.transform.SetPositionAndRotation(liftedPosition, previewRotation);

        // Enable the hologram if it's currently disabled
        if (!currentHologram.activeSelf)
            currentHologram.SetActive(true);
    }

    // Called when preview input is released or canceled
    private void HidePreview()
    {
        if (currentHologram != null)
            currentHologram.SetActive(false);
    }

    // Called when player confirms gravity direction
    // Applies the preview rotation to the actual player
    private void ApplyHologramRotation()
    {
        if (currentHologram != null && playerTransform != null)
        {
            playerTransform.rotation = previewRotation;
            HidePreview();
        }
    }
}
