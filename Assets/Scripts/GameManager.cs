using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public MatchBoard matchBoard;
    public TextMeshProUGUI levelText;

    void Awake()
    {
        instance = this;
    }

    public void GameOver()
    {
        UIManager.Instance.ShowPopup(ScreenType.GameOver);
        Debug.Log("Game Over");
        Time.timeScale = 0f;
    }

    public void LevelComplete()
    {
        UIManager.Instance.ShowPopup(ScreenType.LevelCompleted);
        Debug.Log("Level Completed");
        Time.timeScale = 0f;
    }

    public void ReplayGame()
    {
        Time.timeScale = 1f;
        Debug.Log(Time.timeScale);

        int currentLevel = PlayerPrefs.GetInt("Level", 0);

        MatchBoard.instance.ResetBoard();
        MatchBoardMatch.instance.ResetBoardState();

        LevelManager.instance.LoadLevel(currentLevel);

        UIManager.Instance.HidePopup(ScreenType.GameOver);
        UIManager.Instance.Show(ScreenType.GamePlay);
    }

    public void UpdateLevelText(int levelIndex)
    {
        levelText.text = "Level " + (levelIndex + 1);
    }
}