using System.Collections.Generic;
using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{
    public static ProceduralLevelGenerator instance;
    private SavedLevelsData savedData;
    private Dictionary<int, ProceduralLevelData> generatedLevels
    = new Dictionary<int, ProceduralLevelData>();

    void Awake()
    {
        instance = this;

        savedData = LevelSaveSystem.Load();

        foreach (SavedLevelEntry entry in savedData.levels)
        {
            generatedLevels[entry.levelNumber] = entry.levelData;
        }
    }

    public ProceduralLevelData GenerateLevel(int level)
    {
        if (generatedLevels.ContainsKey(level))
        {
            return generatedLevels[level];
        }

        ProceduralLevelData data = new ProceduralLevelData();

        //difficulty
        data.layers = GetLayerCount(level);

        data.spacing = 130f;


        List<ShapeData> availableShapes = new List<ShapeData>();
        if (availableShapes.Count == 0)
        {
            availableShapes.AddRange(ShapeLoader.database.shapes);
        }

        int maxDifficulty = GetDifficulty(level);

        foreach (ShapeData shapeData in ShapeLoader.database.shapes)
        {
            if (shapeData.difficulty <= maxDifficulty)
            {
                availableShapes.Add(shapeData);
            }
        }

        ShapeData shape = availableShapes[Random.Range(0, availableShapes.Count)];
        data.layout = shape.layout;

        data.rows = shape.layout.Length;
        data.cols = shape.layout[0].Length;

        Debug.Log("Generated shape: " + shape.shapeName);

        generatedLevels.Add(level, data);

        SavedLevelEntry entry = new SavedLevelEntry();
        entry.levelNumber = level;
        entry.levelData = data;

        savedData.levels.Add(entry);
        LevelSaveSystem.Save(savedData);

        Debug.Log("Generating Level Index: " + level);
        return data;
    }

    int GetDifficulty(int level)
    {
        if (level < 5)
            return 1;

        if (level < 15)
            return 2;

        if (level < 30)
            return 3;

        return 4;
    }

    int GetLayerCount(int level)
    {
        if (level < 10)
            return 1;

        if (level < 25)
            return 2;

        if (level < 50)
            return 3;

        if (level < 100)
            return 4;

        return 5;
    }
}