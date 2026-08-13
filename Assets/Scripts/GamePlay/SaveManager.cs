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

        MigrateBoosterUnlockFlags(data);
    }

    /// <summary>
    /// Cleans up inflated booster counts from duplicate unlock grants
    /// and daily rewards earned before the booster was unlocked.
    /// </summary>
    private void MigrateBoosterUnlockFlags(GameData saveData)
    {
        if (saveData == null)
            return;

        bool changed = false;
        int displayLevel = saveData.level + 1;

        if (displayLevel < 3 && saveData.undoCount != 0)
        {
            saveData.undoCount = 0;
            changed = true;
        }

        if (displayLevel < 5 && saveData.shuffleCount != 0)
        {
            saveData.shuffleCount = 0;
            changed = true;
        }

        if (displayLevel < 7 && saveData.magicCount != 0)
        {
            saveData.magicCount = 0;
            changed = true;
        }

        // Duplicate unlock grants before the first unlock tutorial.
        if (displayLevel >= 3 &&
            saveData.undoAnimPlayed == 0 &&
            saveData.undoUnlocked == 0 &&
            saveData.undoCount > 0)
        {
            saveData.undoCount = 0;
            changed = true;
        }

        if (displayLevel >= 5 &&
            saveData.shuffleAnimPlayed == 0 &&
            saveData.shuffleUnlocked == 0 &&
            saveData.shuffleCount > 0)
        {
            saveData.shuffleCount = 0;
            changed = true;
        }

        if (displayLevel >= 7 &&
            saveData.magicAnimPlayed == 0 &&
            saveData.magicUnlocked == 0 &&
            saveData.magicCount > 0)
        {
            saveData.magicCount = 0;
            changed = true;
        }

        // Backfill unlock flags for saves that already finished the unlock tutorial.
        if (displayLevel >= 3 &&
            saveData.undoUnlocked == 0 &&
            saveData.undoAnimPlayed == 1)
        {
            saveData.undoUnlocked = 1;
            changed = true;
        }

        if (displayLevel >= 5 &&
            saveData.shuffleUnlocked == 0 &&
            saveData.shuffleAnimPlayed == 1)
        {
            saveData.shuffleUnlocked = 1;
            changed = true;
        }

        if (displayLevel >= 7 &&
            saveData.magicUnlocked == 0 &&
            saveData.magicAnimPlayed == 1)
        {
            saveData.magicUnlocked = 1;
            changed = true;
        }

        if (changed)
        {
            SaveData();
        }
    }
}