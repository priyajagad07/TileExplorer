using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SimpleShapeEditor : EditorWindow
{
    private int rows = 8;
    private int cols = 8;

    private bool[,] grid;

    private string shapeName = "NewShape";

    private int difficulty = 1;

    [MenuItem("Tools/Simple Shape Editor")]
    public static void ShowWindow()
    {
        GetWindow<SimpleShapeEditor>(
            "Shape Editor"
        );
    }

    void OnEnable()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new bool[rows, cols];
    }

    void OnGUI()
    {
        GUILayout.Label(
            "Shape Settings",
            EditorStyles.boldLabel
        );

        shapeName = EditorGUILayout.TextField(
            "Shape Name",
            shapeName
        );

        difficulty = EditorGUILayout.IntSlider(
            "Difficulty",
            difficulty,
            1,
            5
        );

        int newRows = EditorGUILayout.IntSlider(
            "Rows",
            rows,
            1,
            9
        );

        int newCols = EditorGUILayout.IntSlider(
            "Cols",
            cols,
            1,
            9
        );

        if (newRows != rows || newCols != cols)
        {
            rows = newRows;
            cols = newCols;

            CreateGrid();
        }

        GUILayout.Space(10);

        DrawGrid();

        GUILayout.Space(20);

        if (GUILayout.Button("Clear Grid"))
        {
            CreateGrid();
        }

        if (GUILayout.Button("Save Shape"))
        {
            SaveShape();
        }
    }

    void DrawGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            GUILayout.BeginHorizontal();

            for (int c = 0; c < cols; c++)
            {
                GUI.backgroundColor =
                    grid[r, c]
                    ? Color.green
                    : Color.gray;

                if (
                    GUILayout.Button(
                        "",
                        GUILayout.Width(30),
                        GUILayout.Height(30)
                    )
                )
                {
                    grid[r, c] =
                        !grid[r, c];
                }
            }

            GUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;
    }

    void SaveShape()
    {
        List<string> layout =
            new List<string>();

        for (int r = 0; r < rows; r++)
        {
            string row = "";

            for (int c = 0; c < cols; c++)
            {
                row += grid[r, c]
                    ? "1"
                    : "0";
            }

            layout.Add(row);
        }

        ShapeData shape =
            ScriptableObject.CreateInstance<ShapeData>();

        shape.shapeName = shapeName;

        shape.layout =
            layout.ToArray();

        shape.difficulty =
            difficulty;

        string folder =
            "Assets/Resources/Shapes";

        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder(
                "Assets/Resources",
                "Shapes"
            );
        }

        string path =
            folder + "/" +
            shapeName + ".asset";

        AssetDatabase.CreateAsset(
            shape,
            path
        );

        AssetDatabase.SaveAssets();

        ShapeDatabase database =
            AssetDatabase.LoadAssetAtPath<ShapeDatabase>(
            "Assets/Resources/ShapeDatabase.asset"
        );

        if (database != null)
        {
            database.shapes.Add(shape);

            EditorUtility.SetDirty(database);

            AssetDatabase.SaveAssets();
        }

        AssetDatabase.Refresh();

        Debug.Log(
            "Shape Saved: " + path
        );

        CreateGrid();

        shapeName = "NewShape";

        difficulty = 1;
    }
}