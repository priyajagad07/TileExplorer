using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        GUILayout.Label(
            "Level Preview",
            EditorStyles.boldLabel
        );

        LevelData level =
            (LevelData)target;

        float cellSize = 22f;

        Color[] layerColors =
        {
            new Color(0.4f, 1f, 0.4f),
            new Color(0.4f, 0.8f, 1f),
            new Color(1f, 0.6f, 0.6f),
            new Color(1f, 1f, 0.5f),
            new Color(1f, 0.5f, 1f)
        };

        Rect previewRect =
            GUILayoutUtility.GetRect(
                400,
                400
            );

        for (int layer = 0;
             layer < level.layers.Count;
             layer++)
        {
            ShapeData shape =
                level.layers[layer];

            if (shape == null)
                continue;

            string[] layout =
                shape.layout;

            Color color =
                layerColors[
                    layer % layerColors.Length
                ];

            for (int row = 0;
                 row < layout.Length;
                 row++)
            {
                for (int col = 0;
                     col < layout[row].Length;
                     col++)
                {
                    if (layout[row][col] == '1')
                    {
                        float x =
                            previewRect.x +
                            (col * cellSize) +
                            (layer * 6);

                        float y =
                            previewRect.y +
                            (row * cellSize) -
                            (layer * 6);

                        EditorGUI.DrawRect(
                            new Rect(
                                x,
                                y,
                                cellSize - 2,
                                cellSize - 2
                            ),
                            color
                        );
                    }
                }
            }
        }
    }
}