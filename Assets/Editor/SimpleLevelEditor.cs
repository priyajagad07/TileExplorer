using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SimpleLevelEditor : EditorWindow
{
    private string levelName = "Level_1";

    private int layerCount = 3;

    private List<ShapeData> selectedShapes =
        new List<ShapeData>();

    private ShapeDatabase database;

    [MenuItem("Tools/Simple Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<SimpleLevelEditor>(
            "Level Editor"
        );
    }

    void OnEnable()
    {
        database =
            AssetDatabase.LoadAssetAtPath
            <ShapeDatabase>(
                "Assets/Resources/ShapeDatabase.asset"
            );

        RefreshLayers();
    }

    void RefreshLayers()
    {
        while (
            selectedShapes.Count < layerCount
        )
        {
            selectedShapes.Add(null);
        }

        while (
            selectedShapes.Count > layerCount
        )
        {
            selectedShapes.RemoveAt(
                selectedShapes.Count - 1
            );
        }
    }

    void OnGUI()
    {
        GUILayout.Label(
            "Level Settings",
            EditorStyles.boldLabel
        );

        levelName =
            EditorGUILayout.TextField(
                "Level Name",
                levelName
            );

        int newLayerCount =
            EditorGUILayout.IntSlider(
                "Layers",
                layerCount,
                1,
                5
            );

        if (newLayerCount != layerCount)
        {
            layerCount = newLayerCount;

            RefreshLayers();
        }

        GUILayout.Space(10);

        for (int i = 0; i < layerCount; i++)
        {
            selectedShapes[i] =
                (ShapeData)
                EditorGUILayout.ObjectField(
                    "Layer " + i,
                    selectedShapes[i],
                    typeof(ShapeData),
                    false
                );
        }

        GUILayout.Space(20);


        DrawLevelPreview();

        if (GUILayout.Button("Save Level"))
        {
            SaveLevel();
        }
    }

    void SaveLevel()
    {
        LevelData level =
            ScriptableObject.CreateInstance
            <LevelData>();

        level.layers =
            new List<ShapeData>(
                selectedShapes
            );

        string path =
            "Assets/Resources/Levels/" +
            levelName + ".asset";

        AssetDatabase.CreateAsset(
            level,
            path
        );

        AssetDatabase.SaveAssets();

        LevelDatabase database =
        AssetDatabase.LoadAssetAtPath<LevelDatabase>(
        "Assets/Resources/LevelDatabase.asset"
        );

        if (database != null)
        {
            database.levels.Add(level);

            EditorUtility.SetDirty(database);

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            Debug.Log(
                "Added to LevelDatabase"
            );
        }

        AssetDatabase.Refresh();

        Debug.Log(
            "Level Saved: " + path
        );

        levelName = "Level_1";

        layerCount = 3;

        selectedShapes.Clear();

        RefreshLayers();
    }

    void DrawLevelPreview()
    {
        GUILayout.Label(
            "Level Preview",
            EditorStyles.boldLabel
        );

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
             layer < selectedShapes.Count;
             layer++)
        {
            ShapeData shape =
                selectedShapes[layer];

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