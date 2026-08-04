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
    private int currentLevelDifficulty = 1;

    public bool returnToHomeAfterMap = false;

    [Header("Hard Level Intro")]
    public GameObject hardLevelPanel;
    public TextMeshProUGUI hardlevelText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DOTween.SetTweensCapacity(1000, 300);
    }

    public void SetLevelDifficulty(int difficulty)
    {
        currentLevelDifficulty = difficulty;
    }

    public void StartGame()
    {
        isGameInProgress = true;
        ApplyDifficultyUI(currentLevelDifficulty);
    }

    public void GameOver()
    {
        if (!isGameInProgress)
            return;

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
                CoinManager.instance.AddCoins(50);

                int currentLevel = SaveManager.instance.data.level + 1;
                WorldData currentWorld = BackgroundManager.Instance.GetCurrentWorld();
                WorldData nextWorld = WorldManager.Instance.GetWorldForLevel(currentLevel + 1);

                bool worldChanging = nextWorld != currentWorld;
                bool willUnlock = BackgroundManager.Instance.IsNextDestinationUnlock() || worldChanging;

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
        Time.timeScale = 1f;
        UIManager.Instance.ReturnToGameplayFromPopup();
    }

    public void RestartMidGame()
    {
        UIManager.Instance.HidePopup(ScreenType.ContinueGame);
        ReplayGame();
    }

    public void ReplayGame()
    {
        Debug.Log("========== REPLAY GAME ==========");

        Time.timeScale = 1f;

        // Stop previous gameplay state first.
        isGameInProgress = false;

        ResetLevelState();

        // Clear match-board-specific state.
        MatchBoardMatch.instance.ResetBoardState();
        BoosterSystem.instance.ClearUndoStack();
        MatchBoard.instance.ResetBoard();

        // DO NOT call BoardSpawner.ClearBoard() here.
        // LoadLevel -> SetProceduralLevel already does it.

        int currentLevel = SaveManager.instance.data.level;

        LevelManager.instance.LoadLevel(currentLevel);

        UIManager.Instance.ReturnToGameplayFromPopup();

        DOVirtual.DelayedCall(0.1f, () =>
        {
            if (BoardSpawner.instance != null)
            {
                BoardSpawner.instance.PlaySpawnAnimation();
            }
        });

        Debug.Log("========== REPLAY COMPLETE ==========");
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

        // ProgressUI progressUI = FindAnyObjectByType<ProgressUI>();
        // if (progressUI != null) progressUI.Refresh();
        ProgressUI.RefreshAll();
    }

    private void CompleteLevelReward(int coinAmount)
    {
        SoundManager.instance.PlaySound(SoundName.CoinReach);
        CoinManager.instance.AddCoins(coinAmount);

        int currentLevel = SaveManager.instance.data.level + 1;
        WorldData currentWorld = BackgroundManager.Instance.GetCurrentWorld();
        WorldData nextWorld = WorldManager.Instance.GetWorldForLevel(currentLevel + 1);

        bool worldChanging = nextWorld != currentWorld;
        bool willUnlock = BackgroundManager.Instance.IsNextDestinationUnlock() || worldChanging;

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

    public void ClaimDoubleReward()
    {
        if (rewardClaimed)
            return;

        SoundManager.instance.PlaySound(
            SoundName.ButtonPop
        );

        bool adStarted =
            AdManager.instance.ShowRewardedAd(
                // SUCCESS
                () =>
                {
                    rewardClaimed = true;
                    claimButton.interactable = false;

                    DOVirtual.DelayedCall(0.2f, () =>
                    {
                        SoundManager.instance.PlaySound(
                            SoundName.Coins
                        );

                        if (LevelManager.instance.nextLevelParticles != null)
                        {
                            LevelManager.instance
                                .nextLevelParticles.Play();
                        }

                        SoundManager.instance.PlayHaptic(
                            MOST_HapticFeedback
                                .HapticTypes.Success
                        );

                        DOVirtual.DelayedCall(
                            1.1f,
                            () => CompleteLevelReward(200)
                        );
                    });
                },

                // FAILED / CLOSED WITHOUT REWARD
                () =>
                {
                    claimButton.interactable = true;

                    Debug.Log(
                        "Double reward ad was not completed."
                    );
                }
            );

        if (adStarted)
        {
            claimButton.interactable = false;
        }
        else
        {
            claimButton.interactable = true;

            Debug.Log(
                "Rewarded ad is not ready."
            );
        }
    }
    public void ClaimWinCoins()
    {
        if (rewardClaimed) return;

        rewardClaimed = true;
        //claimButton.interactable = false;

        SoundManager.instance.PlaySound(SoundName.Coins);

        if (LevelManager.instance.nextLevelParticles != null)
        {
            LevelManager.instance.nextLevelParticles.Play();
        }

        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);

        DOVirtual.DelayedCall(1.1f, () => CompleteLevelReward(50));
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

    public void ApplyDifficultyUI(int difficulty)
    {
        if (difficulty < 5) return;

        hardlevelText.text = "HARD LEVEL";

        hardLevelPanel.SetActive(true);
        CanvasGroup cg = hardLevelPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = hardLevelPanel.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        hardlevelText.transform.localScale = Vector3.one;
        Sequence introSeq = DOTween.Sequence();

        introSeq.Append(cg.DOFade(1f, 0.3f));
        introSeq.Join(hardlevelText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 6, 1));
        introSeq.AppendInterval(1.2f);
        introSeq.Append(cg.DOFade(0f, 0.3f));

        introSeq.OnComplete(() =>
        {
            hardLevelPanel.SetActive(false);
        });
    }

    public void ResumeGameAfterRevive()
    {
        Time.timeScale = 1f;

        UIManager.Instance.ReturnToGameplayFromPopup();

        isGameInProgress = true;

        if (MatchBoard.instance != null)
        {
            MatchBoard.instance.isInputLocked = false;
        }

        Debug.Log("GAME RESUMED AFTER REVIVE");
    }
}