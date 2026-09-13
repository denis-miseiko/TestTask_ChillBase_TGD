using UnityEngine;

public class WaypointArrow : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private WaypointManager waypointManager;
    [HideInInspector] private Transform owner;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private float positionSmoothing = 12f;

    [Header("Rotation")]
    [SerializeField] private bool horizontalOnly = true;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Visibility")]
    [Tooltip("Hide the arrow, when there are no waypoints.")]
    [SerializeField] private bool hideWhenNoWaypoint = true;

    [Tooltip("Turn off the GameObject, when there are no waypoints.")]
    [SerializeField] private bool disableGameObjectWhenHidden = false;

    private Transform targetWaypoint;
    private Transform visualRoot;

    private void Start()
    {
        if (waypointManager == null)
            waypointManager = FindFirstObjectByType<WaypointManager>();

        if (waypointManager == null)
            Debug.LogWarning("[Arrow] WaypointManager hasn't been find in the scene.");

        if (transform.childCount > 0)
            visualRoot = transform.GetChild(0);
    }

    private void LateUpdate()
    {
        // Определяем текущую точку
        if (waypointManager != null && waypointManager.CurrentWaypoint != null)
            targetWaypoint = waypointManager.CurrentWaypoint.transform;
        else
            targetWaypoint = null;

        // Управляем видимостью
        UpdateVisibility();

        if (targetWaypoint == null) return;

        FollowOwner();
        PointAtWaypoint();
    }

    private void UpdateVisibility()
    {
        bool shouldShow = targetWaypoint != null || !hideWhenNoWaypoint;

        if (disableGameObjectWhenHidden)
        {
            // Полное отключение GameObject
            if (gameObject.activeSelf != shouldShow)
                gameObject.SetActive(shouldShow);
        }
        else
        {
            // Прячем только визуал
            if (visualRoot != null && visualRoot.gameObject.activeSelf != shouldShow)
                visualRoot.gameObject.SetActive(shouldShow);
        }
    }

    private void FollowOwner()
    {
        Transform currentOwner = owner;

        if (currentOwner == null && waypointManager != null)
            currentOwner = waypointManager.ActiveTarget;

        if (currentOwner == null) return;

        Vector3 desiredPos = currentOwner.position + offset;

        if (positionSmoothing <= 0f)
            transform.position = desiredPos;
        else
            transform.position = Vector3.Lerp(
                transform.position, desiredPos,
                Time.deltaTime * positionSmoothing);
    }

    private void PointAtWaypoint()
    {
        if (targetWaypoint == null) return;

        Vector3 toTarget = targetWaypoint.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f) return;

        Quaternion lookRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);

        if (horizontalOnly)
        {
            Vector3 e = lookRot.eulerAngles;
            lookRot = Quaternion.Euler(0f, e.y, 0f);
        }

        if (rotationSpeed <= 0f)
            transform.rotation = lookRot;
        else
            transform.rotation = Quaternion.Slerp(
                transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
    }
}