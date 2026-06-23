using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public MatchBoard matchBoard;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelTextHomeScreen;
    public GameObject nextLevelButton;
    public Button claimButton;

    private bool rewardClaimed = false;
    private bool levelCompleted = false;

    [Header("Juice Particles")]
    [SerializeField] private ParticleSystem leftConfetti;
    [SerializeField] private ParticleSystem rightConfetti;

    [SerializeField]
    private UIAnimations birdAnimation;

    public bool isGameInProgress = false;

    public bool returnToHomeAfterMap = false;

    void Awake()
    {
        instance = this;
    }

    public void StartGame()
    {
        isGameInProgress = true;
    }

    public void GameOver()
    {
        isGameInProgress = false;
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Warning);
        SoundManager.instance.PlaySound(SoundName.GameOver);
        UIManager.Instance.ShowPopup(ScreenType.GameOver);
        Debug.Log("Game Over");
        Time.timeScale = 0f;
    }

    public void LevelComplete()
    {
        if (levelCompleted) return;

        isGameInProgress = false;
        levelCompleted = true;
        rewardClaimed = false;
        claimButton.interactable = true;

        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);
        SoundManager.instance.PlaySound(SoundName.LevelComplete);
        UIManager.Instance.ShowPopup(ScreenType.LevelCompleted);
        LevelManager.instance.UpdateNextButtonText();

        PlayConfetti();
        Debug.Log("Level Completed");
    }

    public void GoHomeAndReset()
    {
        Time.timeScale = 1f;

        if (levelCompleted && !rewardClaimed)
        {
            rewardClaimed = true;

            SoundManager.instance.PlaySound(SoundName.Coins);
            if (LevelManager.instance.nextLevelParticles != null)
            {
                LevelManager.instance.nextLevelParticles.Play();
            }
            SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);

            DOVirtual.DelayedCall(1.1f, () =>
            {
                CoinManager.instance.AddCoins(100);

                int currentLevel = SaveManager.instance.data.level + 1;
                CountryData currentCountry = BackgroundManager.Instance.GetCurrentCountry();
                CountryData nextCountry = CountryManager.Instance.GetCountryForLevel(currentLevel + 1);

                bool countryChanging = nextCountry != currentCountry;
                bool willUnlock = BackgroundManager.Instance.IsNextDestinationUnlock() || countryChanging;

                SaveManager.instance.data.level++;
                SaveManager.instance.SaveData();

                UpdateLevelText(SaveManager.instance.data.level);

                if (willUnlock)
                {
                    returnToHomeAfterMap = true;

                    int nextDestination = BackgroundManager.Instance.GetNextDestinationIndex();
                    LevelManager.instance.skipMapRefresh = true;
                    MapScreenUI.DestinationUnlocker.SetPending(nextDestination);

                    UIManager.Instance.HidePopup(ScreenType.GameOver);
                    UIManager.Instance.HidePopup(ScreenType.LevelCompleted);

                    MatchBoardMatch.instance.ResetBoardState();
                    BoosterSystem.instance.ClearUndoStack();
                    MatchBoard.instance.ResetBoard();
                    BoardSpawner.instance.ClearBoard();

                    UIManager.Instance.Show(ScreenType.MapScreen);

                    DOVirtual.DelayedCall(0.8f, () =>
                    {
                        MapScreenUI.instance.PlayPendingUnlock();
                    });

                    ResetLevelState();
                }
                else
                {
                    ExecuteHomeTransition();
                }
            });

            return;
        }

        ExecuteHomeTransition();
    }

    private void ExecuteHomeTransition()
    {
        UIManager.Instance.HidePopup(ScreenType.GameOver);
        UIManager.Instance.HidePopup(ScreenType.LevelCompleted);

        MatchBoardMatch.instance.ResetBoardState();
        BoosterSystem.instance.ClearUndoStack();
        MatchBoard.instance.ResetBoard();
        BoardSpawner.instance.ClearBoard();

        UIManager.Instance.Show(ScreenType.HomeScreen);
        ResetLevelState();
    }

    public void ContinueMidGame()
    {
        UIManager.Instance.HidePopup(ScreenType.ContinueGame);
        UIManager.Instance.Show(ScreenType.GamePlay);
    }

    public void RestartMidGame()
    {
        UIManager.Instance.HidePopup(ScreenType.ContinueGame);
        ReplayGame();
    }

    public void ReplayGame()
    {
        ResetLevelState();
        Time.timeScale = 1f;

        MatchBoardMatch.instance.ResetBoardState();
        BoosterSystem.instance.ClearUndoStack();
        MatchBoard.instance.ResetBoard();
        BoardSpawner.instance.ClearBoard();

        int currentLevel = SaveManager.instance.data.level;

        LevelManager.instance.LoadLevel(currentLevel);
        UIManager.Instance.HidePopup(ScreenType.GameOver);
        UIManager.Instance.HidePopup(ScreenType.ContinueGame);
        UIManager.Instance.Show(ScreenType.GamePlay);

        DOVirtual.DelayedCall(0.1f, () =>
        {
            if (BoardSpawner.instance != null)
            {
                BoardSpawner.instance.PlaySpawnAnimation();
            }
        });
    }

    public void UpdateLevelText(int levelIndex)
    {
        levelText.text = "Level " + (levelIndex + 1);
        levelTextHomeScreen.text = "Level " + (levelIndex + 1);

        if (BoosterManager.instance != null)
        {
            BoosterManager.instance.CheckUnlockRewards();
            BoosterManager.instance.UpdateUI();
        }
    }

    private void CompleteLevelReward(int coinAmount)
    {
        SoundManager.instance.PlaySound(SoundName.CoinReach);
        CoinManager.instance.AddCoins(coinAmount);

        int currentLevel = SaveManager.instance.data.level + 1;
        CountryData currentCountry = BackgroundManager.Instance.GetCurrentCountry();
        CountryData nextCountry = CountryManager.Instance.GetCountryForLevel(currentLevel + 1);

        bool countryChanging = nextCountry != currentCountry;
        bool willUnlock = BackgroundManager.Instance.IsNextDestinationUnlock() || countryChanging;

        if (willUnlock)
        {
            int nextDestination = BackgroundManager.Instance.GetNextDestinationIndex();

            LevelManager.instance.skipMapRefresh = true;
            LevelManager.instance.loadLevelSilently = true;
            LevelManager.instance.NextLevel(false);

            MapScreenUI.DestinationUnlocker.SetPending(nextDestination);
            UIManager.Instance.Show(ScreenType.MapScreen);

            DOVirtual.DelayedCall(0.8f, () =>
            {
                MapScreenUI.instance.PlayPendingUnlock();
            });
        }
        else
        {
            LevelManager.instance.NextLevel(false);
        }
    }

    public void ClaimReward()
    {
        if (rewardClaimed) return;

        rewardClaimed = true;
        claimButton.interactable = false;

        SoundManager.instance.PlaySound(SoundName.ButtonPop);

        AdManager.instance.ShowRewardedAd(() =>
        {
            DOVirtual.DelayedCall(0.2f, () =>
            {
                SoundManager.instance.PlaySound(SoundName.Coins);

                if (LevelManager.instance.nextLevelParticles != null)
                {
                    LevelManager.instance.nextLevelParticles.Play();
                }
                SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);

                DOVirtual.DelayedCall(1.1f, () => CompleteLevelReward(200));
            });
        });
    }
    public void ClaimWinCoins()
    {
        if (rewardClaimed) return;

        rewardClaimed = true;

        SoundManager.instance.PlaySound(SoundName.Coins);

        if (LevelManager.instance.nextLevelParticles != null)
        {
            LevelManager.instance.nextLevelParticles.Play();
        }

        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);

        DOVirtual.DelayedCall(1.1f, () => CompleteLevelReward(100));
    }

    public void ResetLevelState()
    {
        levelCompleted = false;
        rewardClaimed = false;

        if (IdleHintManager.instance != null)
        {
            IdleHintManager.instance.ResetIdleTimer();
        }
    }

    void PlayConfetti()
    {
        leftConfetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        rightConfetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        leftConfetti.Play();
        rightConfetti.Play();
    }
}