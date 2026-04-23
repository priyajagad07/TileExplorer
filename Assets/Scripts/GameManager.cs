using UnityEngine;

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
        Debug.Log("Game Over");
        Time.timeScale = 0f;
    }

    public void LevelComplete()
    {
        Debug.Log("Level Completed");
        Time.timeScale = 0f;
    }
}