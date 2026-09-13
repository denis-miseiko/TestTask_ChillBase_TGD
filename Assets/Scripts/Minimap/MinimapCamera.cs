using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [Header("Follow")]
    [Tooltip("Waypoint Manager")]
    [SerializeField] private WaypointManager waypointManager;

    [Tooltip("Backup target")]
    [SerializeField] private Transform fallbackTarget;

    [Tooltip("Camera")]
    [SerializeField] private Transform orientationSource;

    [Header("Position")]
    [SerializeField] private float height = 50f;
    [SerializeField] private float positionSmoothing = 10f;

    [Header("Rotation")]
    [SerializeField] private float rotationSmoothing = 10f;

    private void LateUpdate()
    {
        // ---- Определяем цель ----
        Transform target = null;

        if (waypointManager != null)
            target = waypointManager.ActiveTarget;

        if (target == null)
            target = fallbackTarget;

        if (target == null) return;

        // ---- Позиция над целью ----
        Vector3 desiredPos = new Vector3(
            target.position.x,
            target.position.y + height,
            target.position.z);

        if (positionSmoothing <= 0f)
            transform.position = desiredPos;
        else
            transform.position = Vector3.Lerp(
                transform.position, desiredPos,
                Time.deltaTime * positionSmoothing);

        // ---- Вращение по orientationSource ----
        Transform rotSource = orientationSource != null ? orientationSource : target;

        Vector3 forward = rotSource.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f) return;
        forward.Normalize();

        float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        Quaternion desiredRot = Quaternion.Euler(90f, yaw, 0f);

        if (rotationSmoothing <= 0f)
            transform.rotation = desiredRot;
        else
            transform.rotation = Quaternion.Slerp(
                transform.rotation, desiredRot,
                Time.deltaTime * rotationSmoothing);
    }
}