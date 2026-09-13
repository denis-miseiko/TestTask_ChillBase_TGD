using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    [Header("Links")]
    [Tooltip("Waypoint")]
    [SerializeField] private Transform target;

    [Tooltip("Minimap camera")]
    [SerializeField] private Camera minimapCamera;

    [Tooltip("Waypoint manager")]
    [SerializeField] private WaypointManager waypointManager;

    [Header("Settings")]
    [SerializeField] private float height = 20f;
    [SerializeField] private float edgeFactor = 0.9f;
    [SerializeField] private bool stayOnTargetWhenClose = true;

    public void SetTarget(Transform t) { target = t; }
    public void SetCamera(Camera cam) { minimapCamera = cam; }
    public void SetWaypointManager(WaypointManager mgr) { waypointManager = mgr; }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Определяем центр миникарты - игрок или машина
        Transform center = waypointManager != null
            ? waypointManager.ActiveTarget
            : null;

        // Fallback: если камера или центр не заданы — просто над точкой
        if (minimapCamera == null || center == null)
        {
            transform.position = new Vector3(target.position.x, height, target.position.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            return;
        }

        // Радиус видимой области миникарты в мировых юнитах
        float radius = minimapCamera.orthographicSize * edgeFactor;

        // Вектор от центра к цели в плоскости XZ
        Vector3 fromCenter = target.position - center.position;
        fromCenter.y = 0f;

        float distance = fromCenter.magnitude;
        Vector3 iconPos;

        if (distance <= radius && stayOnTargetWhenClose)
        {
            // Точка внутри миникарты — иконка над самой точкой
            iconPos = new Vector3(target.position.x, height, target.position.z);
        }
        else
        {
            // Точка за пределами — иконка на краю круга в направлении точки
            Vector3 dir = fromCenter.sqrMagnitude > 0.0001f
                ? fromCenter.normalized
                : Vector3.forward;

            Vector3 clamped = center.position + dir * radius;
            iconPos = new Vector3(clamped.x, height, clamped.z);
        }

        transform.position = iconPos;

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}