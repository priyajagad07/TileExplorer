using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public MatchBoard matchBoard;

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

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Debug.Log(Time.timeScale);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}