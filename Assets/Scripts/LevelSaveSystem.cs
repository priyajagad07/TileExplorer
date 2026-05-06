using System.IO;
using UnityEngine;

public static class LevelSaveSystem
{
    private static string savePath = Application.persistentDataPath + "/levels.json";

    public static void Save(SavedLevelsData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Levels saved: " + savePath);
    }

    public static SavedLevelsData Load()
    {
        if (!File.Exists(savePath))
        {
            return new SavedLevelsData();
        }

        string json = File.ReadAllText(savePath);

        return JsonUtility.FromJson<SavedLevelsData>(json);
    }
}