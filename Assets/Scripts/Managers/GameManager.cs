using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public MatchBoard matchBoard;
    public TextMeshProUGUI levelText;
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

        //StartCoroutine(ShowNextButton());
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

    // IEnumerator ShowNextButton()
    // {
    //     yield return new WaitForSecondsRealtime(0.2f);

    //     RectTransform rect = nextLevelButton.GetComponent<RectTransform>();
    //     rect.localScale = Vector3.zero;

    //     float time = 0;
    //     float duration = 0.25f;

    //     while (time < duration)
    //     {
    //         rect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / duration);
    //         time += Time.unscaledDeltaTime;
    //         yield return null;
    //     }

    //     rect.localScale = Vector3.one;
    // }
}