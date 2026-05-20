using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    private int currentLevelIndex;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //PlayerPrefs.DeleteAll();
        currentLevelIndex = PlayerPrefs.GetInt("Level", 0);
        LoadLevel(currentLevelIndex);
        Debug.Log("Saved Level: " + currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        currentLevelIndex = index;
        ProceduralLevelData levelData = ProceduralLevelGenerator.instance.GenerateLevel(index);

        if (levelData == null)
        {
            Debug.LogError("Level data is null");
        }

        BoardGenerator.instance.SetProceduralLevel(levelData);
        GameManager.instance.UpdateLevelText(index);
    }

    public void Nextlevel()
    {
        currentLevelIndex++;

        PlayerPrefs.SetInt("Level", currentLevelIndex);
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        UIManager.Instance.HidePopup(ScreenType.LevelCompleted);
        LoadLevel(currentLevelIndex);
    }
}