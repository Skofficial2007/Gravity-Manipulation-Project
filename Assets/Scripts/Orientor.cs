using UnityEngine;

public class Orientor : MonoBehaviour
{
    public Transform target; // Player root
    public float smoothTime = 0.1f;

    private Vector3 velocity;

    void LateUpdate()
    {
        // Smooth follow
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, smoothTime);

        // Align to gravity (up = opposite of gravity)
        Vector3 gravityUp = -Physics.gravity.normalized;

        // Calculate a forward direction perpendicular to gravity and camera right
        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 forward = Vector3.Cross(gravityUp, cameraRight).normalized;

        // Apply rotation
        transform.rotation = Quaternion.LookRotation(forward, gravityUp);
    }
}
