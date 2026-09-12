using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointManager))]
public class WaypointManagerEditor : Editor
{
    private SerializedProperty pointerPrefabProp;
    private SerializedProperty playerProp;
    private SerializedProperty carProp;

    private GUIStyle duplicateNameStyle;
    private GUIStyle duplicateSummaryStyle;

    private void OnEnable()
    {
        pointerPrefabProp = serializedObject.FindProperty("pointerPrefab");
        playerProp = serializedObject.FindProperty("player");
        carProp = serializedObject.FindProperty("car");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(playerProp);
        EditorGUILayout.PropertyField(carProp);
        EditorGUILayout.PropertyField(pointerPrefabProp);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(8);

        DrawWaypointsList();
    }

    // =====================================================================
    //  СПИСОК ТОЧЕК
    // =====================================================================
    private void DrawWaypointsList()
    {
        var mgr = (WaypointManager)target;
        var list = mgr.Waypoints;

        EditorGUILayout.LabelField(
            "Waypoint",
            EditorStyles.boldLabel);

        // ---- Зона drag-and-drop ----
        Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea,
            "Drag and Drop here",
            EditorStyles.helpBox);
        HandleDragAndDrop(dropArea, mgr);

        EditorGUILayout.Space(4);

        // ---- Карта имён для поиска дубликатов ----
        var nameToIndices = BuildNameIndexMap(list);

        // ---- Строки списка ----
        for (int i = 0; i < list.Count; i++)
        {
            WaypointData wp = list[i];

            string label = wp == null ? null : wp.Label;
            bool isEmptyName = wp != null && string.IsNullOrWhiteSpace(label);
            bool isDuplicate = !string.IsNullOrWhiteSpace(label)
                && nameToIndices.TryGetValue(label, out var dupList)
                && dupList.Count > 1;

            // ---- Подсветка фона ----
            Color prevBg = GUI.backgroundColor;
            if (wp == null)
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            else if (isDuplicate)
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
            else if (isEmptyName)
                GUI.backgroundColor = new Color(1f, 0.9f, 0.5f);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // ---- ID ----
            string idText = wp == null ? "—" : wp.OrderIndex.ToString();
            GUILayout.Label($"ID: {idText}", EditorStyles.miniLabel, GUILayout.Width(50));

            // ---- Имя точки ----
            if (wp == null)
            {
                GUILayout.Label("<empty>", EditorStyles.miniLabel, GUILayout.Width(120));
            }
            else if (isDuplicate)
            {
                EnsureStyles();
                GUILayout.Label($"⚠ {label}", duplicateNameStyle, GUILayout.Width(140));
            }
            else
            {
                GUILayout.Label(label, EditorStyles.miniLabel, GUILayout.Width(120));
            }

            // ---- Слот для замены ссылки ----
            EditorGUI.BeginChangeCheck();
            WaypointData newRef = (WaypointData)EditorGUILayout.ObjectField(
                wp, typeof(WaypointData), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(mgr, "Change Waypoint Reference");
                mgr.RemoveWaypointAt(i);
                if (newRef != null)
                    mgr.InsertWaypoint(i, newRef);
                EditorUtility.SetDirty(mgr);
                GUIUtility.ExitGUI();
            }

            // ---- Выделить в сцене ----
            if (GUILayout.Button("◎", GUILayout.Width(26)) && wp != null)
            {
                Selection.activeGameObject = wp.gameObject;
                EditorGUIUtility.PingObject(wp.gameObject);
                SceneView.lastActiveSceneView?.FrameSelected();
            }

            // ---- Вверх ----
            using (new EditorGUI.DisabledScope(i == 0))
            {
                if (GUILayout.Button("▲", GUILayout.Width(24)))
                {
                    Undo.RecordObject(mgr, "Move Waypoint Up");
                    mgr.MoveWaypoint(i, i - 1);
                    EditorUtility.SetDirty(mgr);
                    GUIUtility.ExitGUI();
                }
            }

            // ---- Вниз ----
            using (new EditorGUI.DisabledScope(i == list.Count - 1))
            {
                if (GUILayout.Button("▼", GUILayout.Width(24)))
                {
                    Undo.RecordObject(mgr, "Move Waypoint Down");
                    mgr.MoveWaypoint(i, i + 1);
                    EditorUtility.SetDirty(mgr);
                    GUIUtility.ExitGUI();
                }
            }

            // ---- Удалить ----
            if (GUILayout.Button("✕", GUILayout.Width(24)))
            {
                Undo.RecordObject(mgr, "Remove Waypoint");
                mgr.RemoveWaypointAt(i);
                EditorUtility.SetDirty(mgr);
                GUI.backgroundColor = prevBg;
                EditorGUILayout.EndHorizontal();
                GUIUtility.ExitGUI();
                return;
            }

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = prevBg;

            // ---- Подсказки ----
            if (wp == null)
            {
                EditorGUILayout.HelpBox(
                    $"Waypoint №{i + 1} empty or doesn't have Waypoint component.",
                    MessageType.Error);
            }
            else if (isEmptyName)
            {
                EditorGUILayout.HelpBox(
                    $"Waypoint №{i + 1} doesn't have name (displayName is empty).",
                    MessageType.Warning);
            }
        }

        EditorGUILayout.Space(4);

        DrawDuplicateSummary(nameToIndices, list);

        EditorGUILayout.Space(4);

        // ---- Кнопки управления списком ----
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("+ Add slot"))
        {
            Undo.RecordObject(mgr, "Add Waypoint Slot");
            mgr.AddEmptySlot();
            EditorUtility.SetDirty(mgr);
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("Collect all waypoints"))
        {
            AutoFillFromScene(mgr);
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.EndHorizontal();
    }

    // =====================================================================
    //  ВСПОМОГАТЕЛЬНОЕ: КАРТА ИМЁН
    // =====================================================================
    private Dictionary<string, List<int>> BuildNameIndexMap(IReadOnlyList<WaypointData> list)
    {
        var map = new Dictionary<string, List<int>>(System.StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < list.Count; i++)
        {
            var wp = list[i];
            if (wp == null) continue;

            string name = wp.Label;
            if (string.IsNullOrWhiteSpace(name)) continue;

            if (!map.TryGetValue(name, out var indices))
            {
                indices = new List<int>();
                map[name] = indices;
            }
            indices.Add(i);
        }

        return map;
    }

    // =====================================================================
    //  СВОДНЫЙ БЛОК ДУБЛИКАТОВ
    // =====================================================================
    private void DrawDuplicateSummary(
        Dictionary<string, List<int>> nameToIndices,
        IReadOnlyList<WaypointData> list)
    {
        var duplicates = nameToIndices
            .Where(kv => kv.Value.Count > 1)
            .ToList();

        if (duplicates.Count == 0) return;

        EditorGUILayout.HelpBox(
            $"Duplicates found: {duplicates.Count}. " +
            "It could crash find system (FindByName / ActivateByName).",
            MessageType.Warning);

        foreach (var dup in duplicates)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EnsureStyles();
            GUILayout.Label(
                $"\"{dup.Key}\" — {dup.Value.Count}.",
                duplicateSummaryStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Highlight", GUILayout.Width(80)))
            {
                var objects = new List<Object>();
                foreach (int idx in dup.Value)
                {
                    if (list[idx] != null) objects.Add(list[idx].gameObject);
                }
                Selection.objects = objects.ToArray();
            }

            if (GUILayout.Button("Fix", GUILayout.Width(80)))
            {
                FixDuplicatesForName(dup.Key, dup.Value, list);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Fix all duplicates"))
        {
            foreach (var dup in duplicates)
                FixDuplicatesForName(dup.Key, dup.Value, list);
            GUIUtility.ExitGUI();
        }
    }

    private void FixDuplicatesForName(
        string baseName, List<int> indices, IReadOnlyList<WaypointData> list)
    {
        for (int k = 1; k < indices.Count; k++)
        {
            var wp = list[indices[k]];
            if (wp == null) continue;

            Undo.RecordObject(wp, "Fix Duplicate Waypoint Name");

            var so = new SerializedObject(wp);
            var nameProp = so.FindProperty("displayName");
            if (nameProp != null)
            {
                nameProp.stringValue = $"{baseName} ({k})";
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(wp);
        }
    }

    // =====================================================================
    //  DRAG-AND-DROP
    // =====================================================================
    private void HandleDragAndDrop(Rect dropArea, WaypointManager mgr)
    {
        Event evt = Event.current;
        if (!dropArea.Contains(evt.mousePosition)) return;

        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();

            Undo.RecordObject(mgr, "Add Waypoints");

            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is not GameObject go) continue;

                var wp = go.GetComponent<WaypointData>()
                         ?? go.GetComponentInChildren<WaypointData>();
                if (wp == null) continue;

                mgr.AddWaypoint(wp); // добавляет в конец + пересчёт ID
            }

            EditorUtility.SetDirty(mgr);
            GUIUtility.ExitGUI();
        }

        evt.Use();
    }

    // =====================================================================
    //  АВТОСБОР: ДОПОЛНЯЕТ СПИСОК, СОХРАНЯЯ ПОРЯДОК
    // =====================================================================
    private void AutoFillFromScene(WaypointManager mgr)
    {
        // 1. Убираем мёртвые ссылки из списка
        int pruned = mgr.PruneMissingWaypoints();
        if (pruned > 0)
        {
            Undo.RecordObject(mgr, "Prune Missing Waypoints");
            EditorUtility.SetDirty(mgr);
            Debug.Log($"[NavigationManager] Missing links deleted: {pruned}");
        }

        // 2. Ищем все точки в сцене
        var found = Object.FindObjectsByType<WaypointData>(FindObjectsSortMode.None);

        if (found.Length == 0)
        {
            Debug.LogWarning("[NavigationManager] The scene doesn't have waypoints.");
            return;
        }

        // 3. Что уже есть в списке (после очистки)
        var existing = new HashSet<WaypointData>(
            mgr.Waypoints.Where(w => w != null));

        // 4. Новые точки
        var newOnes = found.Where(w => w != null && !existing.Contains(w)).ToList();

        if (newOnes.Count == 0)
        {
            Debug.Log("[WaypointManager] All waypoints are already in the list.");
            return;
        }

        // 5. Сортируем новые по имени
        newOnes.Sort((a, b) =>
            string.Compare(a.Label, b.Label, System.StringComparison.Ordinal));

        // 6. Добавляем в конец
        Undo.RecordObject(mgr, "Auto-fill Waypoints");

        foreach (var wp in newOnes)
            mgr.AddWaypoint(wp);

        EditorUtility.SetDirty(mgr);

        Debug.Log($"[WaypointManager] Added new waypoints: {newOnes.Count} " +
                  $"(In the scene: {found.Length}, in the list: {mgr.Waypoints.Count})");
    }

    // =====================================================================
    //  СТИЛИ
    // =====================================================================
    private void EnsureStyles()
    {
        if (duplicateNameStyle == null)
        {
            duplicateNameStyle = new GUIStyle(EditorStyles.miniLabel);
            duplicateNameStyle.normal.textColor = new Color(0.6f, 0f, 0f);
            duplicateNameStyle.fontStyle = FontStyle.Bold;
        }

        if (duplicateSummaryStyle == null)
        {
            duplicateSummaryStyle = new GUIStyle(EditorStyles.boldLabel);
            duplicateSummaryStyle.normal.textColor = new Color(0.6f, 0f, 0f);
        }
    }
}