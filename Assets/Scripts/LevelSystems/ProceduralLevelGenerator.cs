using System.Collections.Generic;
using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{
    public static ProceduralLevelGenerator instance;

    void Awake()
    {
        instance = this;
    }

    public ProceduralLevelData GenerateLevel(int level)
    {
        Random.InitState(level * 1000);

        if (ShapeLoader.database == null || ShapeLoader.database.shapes == null
            || ShapeLoader.database.shapes.Count == 0)
        {
            Debug.LogError("Shape database is Misssing or empty");
            return null;
        }

        ProceduralLevelData data = new ProceduralLevelData();
        //difficulty
        data.layers = GetLayerCount(level);
        data.spacing = 130f;

        List<ShapeData> availableShapes = new List<ShapeData>();

        int maxDifficulty = GetDifficulty(level);

        foreach (ShapeData shapeData in ShapeLoader.database.shapes)
        {
            if (shapeData.difficulty <= maxDifficulty)
            {
                availableShapes.Add(shapeData);
            }
        }

        if (availableShapes.Count == 0)
        {
            Debug.LogWarning("No shapes found for difficulty");
            availableShapes.AddRange(ShapeLoader.database.shapes);
        }

        if (data.layerLayouts == null)
        {
            data.layerLayouts = new List<string[]>();
        }
        else
        {
            data.layerLayouts.Clear();
        }

        for (int layer = 0; layer < data.layers; layer++)
        {
            List<ShapeData> filteredShapes = new List<ShapeData>();

            foreach (ShapeData shapeData in availableShapes)
            {
                int rows = shapeData.layout.Length;
                int cols = shapeData.layout[0].Length;

                if (layer == 0)
                {
                    //bottom layer
                    if (rows >= 6 && cols >= 6)
                    {
                        filteredShapes.Add(shapeData);
                    }
                }

                else if (layer == 1)
                {
                    //medium layer
                    if (rows >= 4 && rows <= 5)
                    {
                        filteredShapes.Add(shapeData);
                    }
                }
                else
                {
                    //small layer
                    if (rows <= 4 && cols <= 5)
                    {
                        filteredShapes.Add(shapeData);
                    }
                }
            }

            if (filteredShapes.Count == 0)
            {
                filteredShapes = availableShapes;
            }

            ShapeData selectedShape = filteredShapes[Random.Range(0, filteredShapes.Count)];
            data.layerLayouts.Add(selectedShape.layout);
        }

        string[] biggestLayout = data.layerLayouts[0];
        data.layout = biggestLayout;
        data.rows = biggestLayout.Length;
        data.cols = biggestLayout[0].Length;

        Debug.Log("Generating Level Index: " + level);
        return data;
    }

    int GetDifficulty(int level)
    {
        if (level < 3)
            return 1;

        if (level < 6)
            return 2;

        if (level < 10)
            return 3;

        if (level < 15)
            return 4;

        return 5;
    }

    int GetLayerCount(int level)
    {
        if (level < 3)
            return 2;

        if (level < 10)
            return 3;

        if (level < 30)
            return 4;

        return 5;
    }
}