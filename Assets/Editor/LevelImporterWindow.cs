using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LevelImporterWindow : EditorWindow
{
    private string levelDataText = "Paste level blueprint here...";

    [MenuItem("Tools/Level Auto-Importer")]
    public static void ShowWindow()
    {
        GetWindow<LevelImporterWindow>("Level Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Level Blueprint Auto-Importer", EditorStyles.boldLabel);
        GUILayout.Label("Paste the text blueprints below to auto-generate shapes and levels.", EditorStyles.wordWrappedLabel);
        
        GUILayout.Space(10);

        levelDataText = EditorGUILayout.TextArea(levelDataText, GUILayout.Height(300));

        GUILayout.Space(10);

        if (GUILayout.Button("Import Level", GUILayout.Height(40)))
        {
            ParseAndImport(levelDataText);
        }
    }

    void ParseAndImport(string text)
    {
        string[] lines = text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        List<ShapeData> currentShapes = new List<ShapeData>();
        ShapeData currentShape = null;
        List<string> currentLayout = new List<string>();

        string levelName = "Imported_Level";
        StackStyle currentStyle = StackStyle.Standard;
        float offX = 30f;
        float offY = 30f;

        // Ensure folders exist
        EnsureFolderExists("Assets/Resources");
        EnsureFolderExists("Assets/Resources/Shapes");
        EnsureFolderExists("Assets/Resources/Levels");

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith("LEVEL"))
            {
                levelName = line.Replace("LEVEL", "").Trim();
            }
            else if (line.StartsWith("STYLE"))
            {
                string styleString = line.Replace("STYLE", "").Trim().ToLower();
                if (styleString == "zigzag") currentStyle = StackStyle.ZigZag;
                else if (styleString == "cascade") currentStyle = StackStyle.Cascade;
                else currentStyle = StackStyle.Standard;
            }
            else if (line.StartsWith("ZIGZAG")) // Backwards compatibility for old imports
            {
                bool isZigZag = bool.Parse(line.Replace("ZIGZAG", "").Trim().ToLower());
                if (isZigZag) currentStyle = StackStyle.ZigZag;
            }
            else if (line.StartsWith("OFFSET"))
            {
                string[] parts = line.Replace("OFFSET", "").Trim().Split(' ');
                if (parts.Length >= 2)
                {
                    float.TryParse(parts[0], out offX);
                    float.TryParse(parts[1], out offY);
                }
            }
            else if (line.StartsWith("LAYER"))
            {
                // Save previous shape if we were building one
                if (currentShape != null && currentLayout.Count > 0)
                {
                    currentShape.layout = currentLayout.ToArray();
                    SaveShapeAsset(currentShape);
                    currentShapes.Add(currentShape);
                }

                currentShape = ScriptableObject.CreateInstance<ShapeData>();
                currentShape.shapeName = line.Replace("LAYER", "").Trim();
                currentShape.difficulty = 1; // Default difficulty
                currentLayout = new List<string>();
            }
            else if (line.Contains("0") || line.Contains("1"))
            {
                // Clean up spaces and add to the row
                string row = line.Replace(" ", "");
                currentLayout.Add(row);
            }
        }

        // Save the very last shape
        if (currentShape != null && currentLayout.Count > 0)
        {
            currentShape.layout = currentLayout.ToArray();
            SaveShapeAsset(currentShape);
            currentShapes.Add(currentShape);
        }

        // Now create and save the LevelData!
        if (currentShapes.Count > 0)
        {
            LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
            newLevel.levelNumber = int.Parse(levelName.Replace("Level_", "").Replace("Level", "").Trim());
            newLevel.layers = currentShapes;
            newLevel.stackStyle = currentStyle;
            newLevel.stackOffsetX = offX;
            newLevel.stackOffsetY = offY;
            newLevel.rewardCoins = 100;

            SaveLevelAsset(newLevel, levelName);
        }
        else
        {
            Debug.LogError("Import Failed! No valid LAYER data found in the text.");
        }
    }

    void SaveShapeAsset(ShapeData shape)
    {
        string path = "Assets/Resources/Shapes/" + shape.shapeName + ".asset";
        AssetDatabase.CreateAsset(shape, path);
        
        ShapeDatabase db = AssetDatabase.LoadAssetAtPath<ShapeDatabase>("Assets/Resources/ShapeDatabase.asset");
        if (db != null && !db.shapes.Contains(shape))
        {
            db.shapes.Add(shape);
            EditorUtility.SetDirty(db);
        }
        AssetDatabase.SaveAssets();
    }

    void SaveLevelAsset(LevelData level, string levelName)
    {
        string path = "Assets/Resources/Levels/" + levelName + ".asset";
        AssetDatabase.CreateAsset(level, path);

        LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/Resources/LevelDatabase.asset");
        if (db != null && !db.levels.Contains(level))
        {
            db.levels.Add(level);
            EditorUtility.SetDirty(db);
        }
        AssetDatabase.SaveAssets();

        Debug.Log("✅ Successfully imported " + levelName + " with " + level.layers.Count + " layers!");
        levelDataText = ""; // Clear box on success
    }

    void EnsureFolderExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path);
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}