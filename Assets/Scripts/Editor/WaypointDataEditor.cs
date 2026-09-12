using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointManager))]
public class WaypointDataEditor : Editor
{
    private SerializedProperty displayNameProp;
    private SerializedProperty orderIndexProp;
    private SerializedProperty triggerRadiusProp;

    private void OnEnable()
    {
        displayNameProp = serializedObject.FindProperty("displayName");
        orderIndexProp = serializedObject.FindProperty("orderIndex");
        triggerRadiusProp = serializedObject.FindProperty("triggerRadius");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var wp = (WaypointData)target;

        // =========================================================
        //  ID Ч первой строкой, только дл€ чтени€
        // =========================================================
        DrawReadOnlyId(wp);

        EditorGUILayout.Space(4);

        // =========================================================
        //  ќстальные пол€
        // =========================================================
        EditorGUILayout.PropertyField(displayNameProp);
        EditorGUILayout.PropertyField(triggerRadiusProp);

        // ѕодсказка, если номер ещЄ не присвоен
        if (wp.OrderIndex == 0)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.HelpBox(
                "ID is not assigned. Add Waypoint to NavigationManager Ч " +
                "ID will be assigned automatically.",
                MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }


    /// –исует строку "ID: N" как заметный read-only блок вверху инспектора.
    private void DrawReadOnlyId(WaypointData wp)
    {
        // ‘он подчЄркивает, что это отдельный блок
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        var boldStyle = new GUIStyle(EditorStyles.boldLabel);
        boldStyle.normal.textColor = EditorGUIUtility.isProSkin
            ? new Color(0.9f, 0.9f, 0.9f)
            : new Color(0.1f, 0.1f, 0.1f);

        GUILayout.Label(
            wp.OrderIndex == 0 ? "ID: Ч" : $"ID: {wp.OrderIndex}",
            boldStyle,
            GUILayout.Width(60));

        var dimStyle = new GUIStyle(EditorStyles.miniLabel);
        dimStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        GUILayout.Label(wp.Label, dimStyle);

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}