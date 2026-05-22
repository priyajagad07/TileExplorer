using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public MatchBoard matchBoard;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelTextHomeScreen;
    public GameObject nextLevelButton;
    public Button claimButton;

    void Awake()
    {
        instance = this;
    }

    public void GameOver()
    {
        SoundManager.instance.PlaySound(SoundName.GameOver);
        UIManager.Instance.ShowPopup(ScreenType.GameOver);
        Debug.Log("Game Over");
        Time.timeScale = 0f;
    }

    public void LevelComplete()
    {
        claimButton.interactable = true;
        SoundManager.instance.PlaySound(SoundName.LevelComplete);
        UIManager.Instance.ShowPopup(ScreenType.LevelCompleted);
        Debug.Log("Level Completed");
        Time.timeScale = 0f;
    }

    public void ReplayGame()
    {
        Time.timeScale = 1f;
        Debug.Log(Time.timeScale);

        MatchBoardMatch.instance.ResetBoardState();

        BoosterSystem.instance.ClearUndoStack();
        MatchBoard.instance.ResetBoard();
        BoardSpawner.instance.ClearBoard();

        int currentLevel = PlayerPrefs.GetInt("Level", 0);

        LevelManager.instance.LoadLevel(currentLevel);

        UIManager.Instance.HidePopup(ScreenType.GameOver);
        UIManager.Instance.Show(ScreenType.GamePlay);
    }

    public void UpdateLevelText(int levelIndex)
    {
        levelText.text = "Level " + (levelIndex + 1);
        levelTextHomeScreen.text = "Level " + (levelIndex + 1);
    }

    public void ClaimReward()
    {
        CoinManager.instance.AddCoins(200);
        claimButton.interactable = false;
    }

    public void ClaimWinCoins()
    {
        CoinManager.instance.AddCoins(100);
    }
}