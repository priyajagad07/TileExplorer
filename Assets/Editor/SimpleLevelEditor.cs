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
}