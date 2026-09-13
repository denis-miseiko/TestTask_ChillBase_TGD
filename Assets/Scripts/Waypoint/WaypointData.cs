using UnityEngine;

public class WaypointData : MonoBehaviour
{
    [Tooltip("Waypoint name. If empty — GameObject name uses.")]
    [SerializeField] private string displayName;

    [Tooltip("ID. Assign automatically.")]
    [SerializeField] private int orderIndex;

    [Tooltip("Interaction radius.")]
    public float triggerRadius = 3f;

    [HideInInspector] public bool isCompleted;

    //id
    public int OrderIndex => orderIndex;

    //name
    public string Label =>
        string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;

    public void SetOrderIndex(int value)
    {
        orderIndex = value;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isCompleted
            ? new Color(0.5f, 0.5f, 0.5f, 0.6f)
            : new Color(0f, 1f, 0f, 0.8f);

        Gizmos.DrawWireSphere(transform.position, triggerRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.color = isCompleted ? Color.gray : Color.green;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (triggerRadius + 0.5f),
            $"{orderIndex}. {Label}"
        );
#endif
    }
}