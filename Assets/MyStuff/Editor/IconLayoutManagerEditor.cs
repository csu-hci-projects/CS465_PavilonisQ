#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IconLayoutManager))]
public class IconLayoutManagerEditor : Editor
{
    private string newLayoutName = "New Layout";
    private int layoutIndexToSave = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        IconLayoutManager manager = (IconLayoutManager)target;

        GUILayout.Space(10);
        GUILayout.Label("Layout Capture Tools", EditorStyles.boldLabel);

        newLayoutName = EditorGUILayout.TextField("Layout Name", newLayoutName);
        layoutIndexToSave = EditorGUILayout.IntField("Layout Index", layoutIndexToSave);

        if (GUILayout.Button("Capture Current Layout"))
        {
            manager.CaptureCurrentLayout(newLayoutName, layoutIndexToSave);
            EditorUtility.SetDirty(manager); // Mark the object as dirty so Unity saves changes
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Test Apply Layout"))
        {
            manager.ApplyLayout(layoutIndexToSave);
        }
    }
}
#endif