using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SimpleLevelEditor : EditorWindow
{
    private string levelName = "Level_1";
    private int layerCount = 3;

    // ---> NEW: Use the Enum instead of a boolean <---
    private StackStyle stackStyle = StackStyle.Standard;
    private float stackOffsetX = 30f;
    private float stackOffsetY = 30f;

    private List<ShapeData> selectedShapes = new List<ShapeData>();
    private ShapeDatabase database;

    [MenuItem("Tools/Simple Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<SimpleLevelEditor>("Level Editor");
    }

    void OnEnable()
    {
        database = AssetDatabase.LoadAssetAtPath<ShapeDatabase>("Assets/Resources/ShapeDatabase.asset");
        RefreshLayers();
    }

    void RefreshLayers()
    {
        while (selectedShapes.Count < layerCount) selectedShapes.Add(null);
        while (selectedShapes.Count > layerCount) selectedShapes.RemoveAt(selectedShapes.Count - 1);
    }

    void OnGUI()
    {
        GUILayout.Label("Level Settings", EditorStyles.boldLabel);

        levelName = EditorGUILayout.TextField("Level Name", levelName);
        int newLayerCount = EditorGUILayout.IntSlider("Layers", layerCount, 1, 5);

        if (newLayerCount != layerCount)
        {
            layerCount = newLayerCount;
            RefreshLayers();
        }

        GUILayout.Space(10);
        GUILayout.Label("Stack Style", EditorStyles.boldLabel);
        
        // ---> NEW: UI Control for the Enum <---
        stackStyle = (StackStyle)EditorGUILayout.EnumPopup("Stack Style", stackStyle);
        stackOffsetX = EditorGUILayout.Slider("Stack Offset X", stackOffsetX, -100f, 100f);
        stackOffsetY = EditorGUILayout.Slider("Stack Offset Y", stackOffsetY, -100f, 100f);

        GUILayout.Space(10);

        for (int i = 0; i < layerCount; i++)
        {
            selectedShapes[i] = (ShapeData)EditorGUILayout.ObjectField("Layer " + i, selectedShapes[i], typeof(ShapeData), false);
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
        LevelData level = ScriptableObject.CreateInstance<LevelData>();

        level.layers = new List<ShapeData>(selectedShapes);
        
        // ---> NEW: Save the Enum <---
        level.stackStyle = stackStyle;
        level.stackOffsetX = stackOffsetX;
        level.stackOffsetY = stackOffsetY;

        string path = "Assets/Resources/Levels/" + levelName + ".asset";
        AssetDatabase.CreateAsset(level, path);
        AssetDatabase.SaveAssets();

        LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/Resources/LevelDatabase.asset");

        if (database != null)
        {
            database.levels.Add(level);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Debug.Log("Added to LevelDatabase");
        }

        AssetDatabase.Refresh();
        Debug.Log("Level Saved: " + path);

        levelName = "Level_1";
        layerCount = 3;
        selectedShapes.Clear();
        RefreshLayers();
    }

    void DrawLevelPreview()
    {
        GUILayout.Label("Level Preview", EditorStyles.boldLabel);
        float cellSize = 22f;
        Color[] layerColors = { new Color(0.4f, 1f, 0.4f), new Color(0.4f, 0.8f, 1f), new Color(1f, 0.6f, 0.6f), new Color(1f, 1f, 0.5f), new Color(1f, 0.5f, 1f) };

        Rect previewRect = GUILayoutUtility.GetRect(400, 400);

        int maxLayers = selectedShapes.Count;
        if (maxLayers == 0) return;
        
        int topLayerIndex = maxLayers - 1;

        float previewScale = cellSize / 130f;
        float previewOffsetX = stackOffsetX * previewScale;
        float previewOffsetY = stackOffsetY * previewScale;

        float centerX = previewRect.x + (previewRect.width / 2f);
        float centerY = previewRect.y + (previewRect.height / 2f);

        for (int layer = 0; layer < maxLayers; layer++)
        {
            ShapeData shape = selectedShapes[layer];
            if (shape == null || shape.layout == null || shape.layout.Length == 0) continue;

            string[] layout = shape.layout;
            Color color = layerColors[layer % layerColors.Length];

            int depthFromTop = topLayerIndex - layer;
            float currentLayerOffsetX = 0f;
            float currentLayerOffsetY = 0f;

            // ---> NEW: Match the BoardSpawner math with Cascade <---
            if (stackStyle == StackStyle.ZigZag)
            {
                if (depthFromTop == 0) currentLayerOffsetX = 0f;
                else if (depthFromTop % 2 == 1) currentLayerOffsetX = -previewOffsetX;
                else currentLayerOffsetX = previewOffsetX;
                
                currentLayerOffsetY = depthFromTop * previewOffsetY; 
            }
            else if (stackStyle == StackStyle.Cascade)
            {
                // Cascade pushes layers diagonally continuously
                currentLayerOffsetX = depthFromTop * previewOffsetX;
                currentLayerOffsetY = depthFromTop * previewOffsetY;
            }
            else
            {
                // Standard Stack pushes opposite way on X to center or lean
                currentLayerOffsetX = depthFromTop * -previewOffsetX;
                currentLayerOffsetY = depthFromTop * previewOffsetY; 
            }

            float currentWidth = (layout[0].Length - 1) * cellSize;
            float currentHeight = (layout.Length - 1) * cellSize;
            
            float currentStartX = centerX - (currentWidth / 2f);
            float currentStartY = centerY - (currentHeight / 2f);

            for (int row = 0; row < layout.Length; row++)
            {
                for (int col = 0; col < layout[row].Length; col++)
                {
                    if (layout[row][col] == '1')
                    {
                        float x = currentStartX + (col * cellSize) + currentLayerOffsetX;
                        float y = currentStartY + (row * cellSize) + currentLayerOffsetY;

                        float drawX = x - (cellSize / 2f);
                        float drawY = y - (cellSize / 2f);

                        EditorGUI.DrawRect(new Rect(drawX, drawY, cellSize - 2, cellSize - 2), color);
                    }
                }
            }
        }
    }
}