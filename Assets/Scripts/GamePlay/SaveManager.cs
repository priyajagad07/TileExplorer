using UnityEngine;
using System.IO;

[DefaultExecutionOrder(-100)]
public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public GameData data;
    private string saveFilePath;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            saveFilePath =
                Path.Combine(
                    Application.persistentDataPath,
                    "savegame.json"
                );

            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SaveData()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
    }

    public void LoadData()
    {
        bool dataChanged = false;

        if (File.Exists(saveFilePath))
        {
            string json =
                File.ReadAllText(saveFilePath);

            data =
                JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            data = new GameData();
            dataChanged = true;
        }

        // Protect against invalid or empty save data.
        if (data == null)
        {
            data = new GameData();
            dataChanged = true;
        }

        // Create the name before any UI screen is shown.
        if (string.IsNullOrWhiteSpace(data.playerName))
        {
            data.playerName =
                "Player" + Random.Range(1000, 10000);

            dataChanged = true;
        }

        if (dataChanged)
        {
            SaveData();
        }
    }
}