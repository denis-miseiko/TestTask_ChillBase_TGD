using UnityEngine;

public class PlayerMinimapIcon : MonoBehaviour
{
    [Header("Links")]
    [Tooltip("Waypoint manager")]
    [SerializeField] private WaypointManager waypointManager;

    [Tooltip("Backup target")]
    [SerializeField] private Transform fallbackTarget;

    [Header("Positioning")]
    [SerializeField] private float height = 20f;

    [Header("Sprite orientation")]
    [SerializeField] private float rotationOffset = -90f;

    [Header("Sprites (optionally)")]
    [SerializeField] private Renderer iconRenderer;
    [SerializeField] private Material pedestrianMaterial;
    [SerializeField] private Material vehicleMaterial;

    public void SetPlayer(Transform t)
    {
        fallbackTarget = t;
    }

    public void SetMinimapCamera(Transform cam)
    {
        // Не используется. Оставлено для совместимости.
    }

    public void SetWaypointManager(WaypointManager mgr)
    {
        waypointManager = mgr;
    }

    private bool lastInCar;

    private void LateUpdate()
    {
        // ---- Определяем цель ----
        Transform target = null;
        bool inCar = false;

        if (waypointManager != null)
        {
            target = waypointManager.ActiveTarget;
            inCar = waypointManager.IsPlayerInCar;
        }

        if (target == null)
            target = fallbackTarget;

        if (target == null) return;

        // ---- Позиция ----
        transform.position = new Vector3(
            target.position.x,
            height,
            target.position.z);

        // ---- Вращение по forward цели ----
        Vector3 fwd = target.forward;
        fwd.y = 0f;

        if (fwd.sqrMagnitude < 0.0001f) return;
        fwd.Normalize();

        float yaw = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90f, yaw + rotationOffset, 0f);

        // ---- Смена спрайта при входе/выходе из машины ----
        if (iconRenderer != null && inCar != lastInCar)
        {
            lastInCar = inCar;
            var mat = inCar ? vehicleMaterial : pedestrianMaterial;
            if (mat != null) iconRenderer.sharedMaterial = mat;
        }
    }
}