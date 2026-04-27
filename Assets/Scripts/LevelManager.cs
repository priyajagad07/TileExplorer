using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public LevelDatabase levelDatabase;
    private int currentLevelIndex;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentLevelIndex = PlayerPrefs.GetInt("Level" , 0);
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        LevelData level = levelDatabase.levels[index];

        BoardGenerator generator = FindAnyObjectByType<BoardGenerator>();
        generator.SetLevel(level);
    }

    public void Nextlevel()
    {
        currentLevelIndex++;

        if(currentLevelIndex >= levelDatabase.levels.Length)
        {
            currentLevelIndex = 0;
        }

        PlayerPrefs.SetInt("Level", currentLevelIndex);
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        LoadLevel(currentLevelIndex);
    }
}