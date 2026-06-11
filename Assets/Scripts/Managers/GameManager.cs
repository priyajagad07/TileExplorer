using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public MatchBoard matchBoard;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelTextHomeScreen;
    public GameObject nextLevelButton;
    public Button claimButton;
    private GameObject coinParticle;
    private bool rewardClaimed = false;
    [SerializeField] private ParticleSystem leftConfetti;
    [SerializeField] private ParticleSystem rightConfetti;
    private bool levelCompleted = false;
    [SerializeField]
    private UIAnimations birdAnimation;

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
        if (levelCompleted)
            return;

        levelCompleted = true;

        rewardClaimed = false;
        claimButton.interactable = true;
        SoundManager.instance.PlaySound(SoundName.LevelComplete);

        DailyStreakManager.instance.OnLevelCompleted();
        UIManager.Instance.ShowPopup(ScreenType.LevelCompleted);
        LevelManager.instance.UpdateNextButtonText();

        PlayConfetti();

        Debug.Log("Level Completed");
    }

    public void ReplayGame()
    {
        ResetLevelState();
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

    private void CompleteLevelReward(int coinAmount)
    {
        SoundManager.instance.PlaySound(SoundName.CoinReach);
        CoinManager.instance.AddCoins(coinAmount);

        if (DailyStreakManager.instance.ShouldShowRewardPopup())
        {
            UIManager.Instance.Show(ScreenType.DailyStreakScreen);
            DOVirtual.DelayedCall(0.1f, () =>
                {
                    DailyStreakUI.instance.OpenDailyReward();
                }
            );
        }
        else
        {
            DailyStreakManager.instance.ClearPendingReward();
            if (BackgroundManager.Instance.IsNextDestinationUnlock())
            {
                int nextDestination =
                    BackgroundManager.Instance
                        .GetNextDestinationIndex();

                MapScreenUI.DestinationUnlocker
                    .SetPending(nextDestination);

                UIManager.Instance.Show(
     ScreenType.MapScreen
 );

                DOVirtual.DelayedCall(
                    0.2f,
                    () =>
                    {
                        MapScreenUI.instance
                            .PlayPendingUnlock();
                    }
                );

                MapScreenUI.instance
                    .PlayPendingUnlock();
            }
            else
            {
                LevelManager.instance.NextLevel(false);
            }
        }
    }

    public void ClaimReward()
    {
        if (rewardClaimed)
            return;

        rewardClaimed = true;
        claimButton.interactable = false;

        SoundManager.instance.PlaySound(SoundName.Coins);
        if (LevelManager.instance.nextLevelParticles != null)
        {
            LevelManager.instance.nextLevelParticles.Play();
        }

        DOVirtual.DelayedCall(1.1f, () =>
        {
            CompleteLevelReward(200);
        }
        );
    }

    public void ClaimWinCoins()
    {
        if (rewardClaimed)
            return;

        rewardClaimed = true;

        SoundManager.instance.PlaySound(SoundName.Coins);
        if (LevelManager.instance.nextLevelParticles != null)
        {
            LevelManager.instance.nextLevelParticles.Play();
        }

        DOVirtual.DelayedCall(1.1f, () =>
        {
            CompleteLevelReward(100);
        }
        );
    }

    public void ResetLevelState()
    {
        levelCompleted = false;
        rewardClaimed = false;
    }

    void PlayConfetti()
    {
        leftConfetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        rightConfetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        leftConfetti.Play();
        rightConfetti.Play();
    }
}