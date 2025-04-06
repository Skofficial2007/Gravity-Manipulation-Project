// At first i was using my own cameraFolllow script for third person camera but
// It didn't work out well with gravity maniplulation so in the end i decide to swift
// I'm using cinemachine freelook camera and orientor to work well with gravity manipulation in game




//using UnityEngine;

//public class CameraManager : MonoBehaviour
//{
//    [Header("Dependencies")]
//    [SerializeField] private InputManager inputManager;
//    [SerializeField] private Transform targetTransform;     // Player transform to follow
//    [SerializeField] private Transform cameraPivot;         // Pivot for vertical pitch rotation
//    [SerializeField] private Transform cameraTransform;     // Actual main camera

//    [Header("Camera Settings")]
//    [SerializeField] private float cameraFollowSpeed = 0.2f;
//    [SerializeField] private float cameraLookSpeed = 0.3f;
//    [SerializeField] private float cameraPivotSpeed = 0.3f;
//    [SerializeField] private float cameraCollisionRadius = 0.5f;
//    [SerializeField] private float cameraCollisionOffset = 0.5f;
//    [SerializeField] private float minimumCollisionOffset = 0.2f;
//    [SerializeField] private LayerMask collisionLayers;

//    private Vector3 cameraFollowVelocity = Vector3.zero;
//    private Vector3 defaultCameraOffset = new Vector3(0, 2.0f, -4.0f); // Used for gravity-aware positioning

//    private float lookAngle;
//    private float pivotAngle;

//    private const float minimumPivotAngle = -35f;
//    private const float maximumPivotAngle = 35f;

//    private bool skipNextRotation = false;
//    private bool skipNextCollision = false;

//    private void Awake()
//    {
//        // Set initial camera position
//        cameraTransform.position = cameraPivot.position + cameraPivot.rotation * defaultCameraOffset;
//    }

//    public void HandleAllCameraMovement()
//    {
//        FollowTarget();
//        RotateCamera();
//        HandleCameraCollisions();
//    }

//    private void FollowTarget()
//    {
//        Vector3 targetPosition = Vector3.SmoothDamp(transform.position, targetTransform.position, ref cameraFollowVelocity, cameraFollowSpeed);
//        transform.position = targetPosition;
//    }

//    private void RotateCamera()
//    {
//        if (skipNextRotation)
//        {
//            skipNextRotation = false;
//            return;
//        }

//        Vector3 gravityUp = -Physics.gravity.normalized;

//        lookAngle += inputManager.GetCameraInputX() * cameraLookSpeed;
//        Quaternion yawRotation = Quaternion.AngleAxis(lookAngle, gravityUp);
//        transform.rotation = yawRotation;

//        pivotAngle -= inputManager.GetCameraInputY() * cameraPivotSpeed;
//        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivotAngle, maximumPivotAngle);

//        Vector3 pitchAxis = cameraPivot.right;
//        Quaternion pitchRotation = Quaternion.AngleAxis(pivotAngle, pitchAxis);
//        cameraPivot.rotation = pitchRotation * transform.rotation;
//    }

//    private void HandleCameraCollisions()
//    {
//        if (skipNextCollision)
//        {
//            skipNextCollision = false;
//            return;
//        }

//        Vector3 desiredCameraPosition = cameraPivot.position + cameraPivot.rotation * defaultCameraOffset;
//        Vector3 direction = (desiredCameraPosition - cameraPivot.position).normalized;
//        float targetDistance = defaultCameraOffset.magnitude;
//        float adjustedDistance = targetDistance;

//        if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, direction, out RaycastHit hit, targetDistance, collisionLayers))
//        {
//            adjustedDistance = hit.distance - cameraCollisionOffset;
//            adjustedDistance = Mathf.Max(adjustedDistance, minimumCollisionOffset);
//        }

//        Vector3 finalCameraWorldPos = cameraPivot.position + direction * adjustedDistance;
//        cameraTransform.position = Vector3.Lerp(cameraTransform.position, finalCameraWorldPos, 0.2f);
//    }

//    public void AlignToGravity(Transform playerTransform)
//    {
//        Vector3 gravityUp = -Physics.gravity.normalized;

//        // Get forward direction relative to new gravity
//        Vector3 projectedForward = Vector3.ProjectOnPlane(playerTransform.forward, gravityUp).normalized;
//        if (projectedForward == Vector3.zero)
//            projectedForward = playerTransform.forward;

//        // Apply new orientation to camera rig and pivot
//        Quaternion newRigRotation = Quaternion.LookRotation(projectedForward, gravityUp);
//        transform.rotation = newRigRotation;
//        cameraPivot.rotation = newRigRotation;

//        // Recalculate gravity-aware camera offset
//        Vector3 cameraOffset = (-projectedForward * 4.0f) + (gravityUp * 2.0f);
//        defaultCameraOffset = cameraOffset;

//        // Force camera to updated position instantly
//        cameraTransform.position = cameraPivot.position + cameraOffset;

//        // Reset angles
//        pivotAngle = 0f;
//        lookAngle = newRigRotation.eulerAngles.y;

//        skipNextRotation = true;
//        skipNextCollision = true;
//    }

//    private void OnDrawGizmos()
//    {
//        Gizmos.color = Color.red;
//        Gizmos.DrawRay(transform.position, transform.right);
//        Gizmos.color = Color.green;
//        Gizmos.DrawRay(transform.position, transform.up);
//        Gizmos.color = Color.blue;
//        Gizmos.DrawRay(transform.position, transform.forward);

//        if (cameraPivot != null && cameraTransform != null)
//        {
//            Gizmos.color = Color.yellow;
//            Gizmos.DrawLine(cameraPivot.position, cameraTransform.position);
//            Gizmos.DrawWireSphere(cameraTransform.position, 0.2f);
//        }
//    }
//}
