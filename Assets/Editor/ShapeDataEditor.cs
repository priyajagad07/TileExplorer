using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShapeData))]
public class ShapeDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ShapeData shape =
            (ShapeData)target;

        GUIStyle style =
            new GUIStyle(EditorStyles.label);

        style.fontSize = 18;

        style.alignment =
            TextAnchor.MiddleCenter;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Shape Name",
            shape.shapeName
        );

        EditorGUILayout.LabelField(
            "Difficulty",
            shape.difficulty.ToString()
        );

        EditorGUILayout.Space();

        if (shape.layout != null)
        {
            foreach (string row in shape.layout)
            {
                GUILayout.BeginHorizontal();

                foreach (char c in row)
                {
                    GUILayout.Label(
                        c == '1'
                        ? "🟩"
                        : "⬜",
                        style,
                        GUILayout.Width(25),
                        GUILayout.Height(25)
                    );
                }

                GUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();

        DrawDefaultInspector();
    }
}