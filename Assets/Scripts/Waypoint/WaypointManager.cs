using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEditor.Build;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [Header("Waypoints")]
    [SerializeField] private List<WaypointData> waypoints = new List<WaypointData>();

    [Header("Marker")]
    [SerializeField] private GameObject pointerPrefab;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Car")]
    [SerializeField] private Transform car;

    private Transform activeTarget = null;

    public event Action<WaypointData> OnWaypointReached;
    public event Action OnAllWaypointsCompleted;

    public WaypointData CurrentWaypoint { get; private set; }
    public int CurrentIndex => currentIndex;
    public IReadOnlyList<WaypointData> Waypoints => waypoints;

    private int currentIndex = -1;
    private GameObject currentPointerInstance;

    [SerializeField] private InOutCar myInCar;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        RenumberWaypoints();
        foreach (var wp in waypoints)
            if (wp != null) wp.isCompleted = false;

        myInCar = FindFirstObjectByType<InOutCar>();
    }

    private void Start()
    {
        ActivateNextWaypoint();
    }

    private void Update()
    {
        activeTarget = ActiveTarget;

        if (CurrentWaypoint == null) return;

        Vector3 a = activeTarget.position; a.y = 0f;
        Vector3 b = CurrentWaypoint.transform.position; b.y = 0f;

        if (Vector3.Distance(a, b) <= CurrentWaypoint.triggerRadius)
            ReachCurrentWaypoint();
    }
    public Transform ActiveTarget
    {
        get
        {
            if (myInCar == null)
            {
                activeTarget = player;
            }
            else
            {
                activeTarget = myInCar.InCar ? car : player;
            }
            return activeTarget;
        }
    }

    private void ActivateNextWaypoint()
    {
        currentIndex++;

        if (currentIndex >= waypoints.Count)
        {
            CurrentWaypoint = null;
            OnAllWaypointsCompleted?.Invoke();
            return;
        }

        CurrentWaypoint = waypoints[currentIndex];

        if (pointerPrefab != null)
        {
            currentPointerInstance = Instantiate(
            pointerPrefab,
            CurrentWaypoint.transform.position + Vector3.up * 2f,
            Quaternion.identity
            );
            currentPointerInstance.SetActive(true);
        }
    }

    private void ReachCurrentWaypoint()
    {
        CurrentWaypoint.isCompleted = true;
        OnWaypointReached?.Invoke(CurrentWaypoint);

        if (currentPointerInstance != null)
            Destroy(currentPointerInstance);

        ActivateNextWaypoint();
    }

    // =====================================================================
    //  Управление списком точек
    // =====================================================================

    public void RenumberWaypoints()
    {
        int id = 1;
        for (int i = 0; i < waypoints.Count; i++)
            if (waypoints[i] != null)
                waypoints[i].SetOrderIndex(id++);
    }

    public void SetWaypoints(List<WaypointData> newWaypoints)
    {
        waypoints = newWaypoints ?? new List<WaypointData>();
        RenumberWaypoints();
    }

    public void AddWaypoint(WaypointData wp)
    {
        if (wp == null) return;
        if (waypoints.Contains(wp)) return;

        waypoints.Add(wp);
        RenumberWaypoints();
    }

    // Добавляет пустой слот в конец списка
    public void AddEmptySlot()
    {
        waypoints.Add(null);
        RenumberWaypoints();
    }

    public void InsertWaypoint(int index, WaypointData wp)
    {
        if (wp == null) return;
        if (waypoints.Contains(wp)) return;

        index = Mathf.Clamp(index, 0, waypoints.Count);
        waypoints.Insert(index, wp);
        RenumberWaypoints();
    }

    public void RemoveWaypointAt(int index)
    {
        if (index < 0 || index >= waypoints.Count) return;

        var removed = waypoints[index];

        waypoints.RemoveAt(index);

        // Сбрасываем ID у удалённой точки
        if (removed != null)
            removed.SetOrderIndex(0);

        RenumberWaypoints();
    }

    public void MoveWaypoint(int from, int to)
    {
        if (from < 0 || from >= waypoints.Count) return;
        if (to < 0 || to >= waypoints.Count) return;
        if (from == to) return;

        var wp = waypoints[from];
        waypoints.RemoveAt(from);
        waypoints.Insert(to, wp);
        RenumberWaypoints();
    }


    // =====================================================================
    //  Поиск и активация по имени
    // =====================================================================

    public WaypointData FindByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        foreach (var wp in waypoints)
            if (wp != null &&
                string.Equals(wp.Label, name, StringComparison.OrdinalIgnoreCase))
                return wp;

        return null;
    }

    public bool ActivateByName(string name)
    {
        int idx = waypoints.FindIndex(w =>
            w != null && string.Equals(w.Label, name, StringComparison.OrdinalIgnoreCase));

        if (idx < 0)
        {
            Debug.LogWarning($"[Nav] Точка с именем \"{name}\" не найдена.");
            return false;
        }

        if (currentPointerInstance != null)
            Destroy(currentPointerInstance);

        currentIndex = idx - 1;
        ActivateNextWaypoint();
        return true;
    }

    public List<string> GetWaypointLabels()
    {
        var list = new List<string>(waypoints.Count);
        for (int i = 0; i < waypoints.Count; i++)
        {
            var wp = waypoints[i];
            list.Add(wp == null ? $"{i + 1}. <пусто>" : $"{wp.OrderIndex}. {wp.Label}");
        }
        return list;
    }
    public int PruneMissingWaypoints()
    {
        int removed = 0;
        for (int i = waypoints.Count - 1; i >= 0; i--)
        {
            // Ссылка мертва: либо null, либо уничтоженный UnityEngine.Object
            if (waypoints[i] == null)
            {
                waypoints.RemoveAt(i);
                removed++;
            }
        }

        if (removed > 0)
            RenumberWaypoints();

        return removed;
    }
}