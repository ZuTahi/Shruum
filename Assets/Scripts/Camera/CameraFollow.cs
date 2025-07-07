using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;            // Player to follow
    public Vector3 offset = new Vector3(0f, 10f, -10f); // Offset for isometric angle
    public float smoothSpeed = 5f;      // How quickly the camera moves

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
        transform.LookAt(target); // Optional: keeps camera angled at player
    }
}
