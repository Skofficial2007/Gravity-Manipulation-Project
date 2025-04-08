using UnityEngine;

public class GravityPreviewManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputManager input;                   // Input system reference for gravity preview events
    [SerializeField] private Transform playerTransform;            // Used to determine hologram's position and orientation
    [SerializeField] private GameObject currentHologram;           // The visual hologram shown during gravity preview

    [Header("Settings")]
    [SerializeField] private float gravityOffsetDistance = 3.5f;   // How far the hologram appears from the player
    [SerializeField] private float liftOffset = 2f;                // How far the hologram is lifted up relative to current gravity

    private Quaternion previewRotation;                            // Stores the rotation the hologram should have
    private Vector3 previewGravityDirection;                       // Direction currently being previewed

    private void OnEnable()
    {
        if (input == null)
        {
            Debug.LogError("GravityPreviewManager: InputManager not assigned!");
            return;
        }

        // Subscribe to gravity preview events from InputManager
        input.OnGravityPreviewed += ShowPreview;
        input.OnGravityPreviewCanceled += HidePreview;
        input.OnGravityPreviewConfirmed += ApplyHologramRotation;
    }

    private void OnDisable()
    {
        if (input == null) return;

        // Unsubscribe to prevent memory leaks or stale listeners
        input.OnGravityPreviewed -= ShowPreview;
        input.OnGravityPreviewCanceled -= HidePreview;
        input.OnGravityPreviewConfirmed -= ApplyHologramRotation;
    }

    // Displays and orients the hologram based on previewed gravity direction
    private void ShowPreview(Vector3 gravityDirection)
    {
        if (playerTransform == null || currentHologram == null)
            return;

        previewGravityDirection = gravityDirection;

        // "Up" direction is opposite gravity
        Vector3 gravityUp = -gravityDirection.normalized;

        // Use camera forward projected onto the gravity plane for orientation
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 projectedForward = Vector3.ProjectOnPlane(cameraForward, gravityUp).normalized;

        // Fallback in case of projection failure
        if (projectedForward == Vector3.zero)
            projectedForward = Vector3.ProjectOnPlane(Vector3.forward, gravityUp);

        // Determine the rotation for the hologram
        previewRotation = Quaternion.LookRotation(projectedForward, gravityUp);

        // Calculate position based on gravity direction and lift offset
        Vector3 basePosition = playerTransform.position + gravityDirection.normalized * gravityOffsetDistance;
        Vector3 liftedPosition = basePosition + (-GravityManager.CurrentGravity.normalized * liftOffset);

        // Set the hologram's position and orientation
        currentHologram.transform.SetPositionAndRotation(liftedPosition, previewRotation);

        // Ensure the hologram is visible
        if (!currentHologram.activeSelf)
            currentHologram.SetActive(true);
    }

    // Hides the hologram if preview is canceled or cleared
    private void HidePreview()
    {
        if (currentHologram != null)
            currentHologram.SetActive(false);
    }

    // Called when player confirms a gravity shift
    // Rotation of player is now handled by GravityManager, so we just hide the preview
    private void ApplyHologramRotation()
    {
        HidePreview();
    }
}
