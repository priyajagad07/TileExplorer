using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    private int currentLevelIndex;

    [Header("Next Level Transition")]
    public UIParticle nextLevelParticles;
    public Button nextLevelButton;
    private bool isLoadingNextLevel = false;

    [SerializeField] private LevelDatabase levelDatabase;
    [SerializeField] private float nextLevelParticleDuration = 1.2f;
    public bool shouldPlaySpawnAnimation;
    public bool loadLevelSilently = false;
    [SerializeField]
    private TMP_Text nextLevelText;
    public bool skipMapRefresh;
    private const int FIRST_INSTALL_FREE_LEVELS = 10;
    private const int DAILY_FREE_LEVELS_MIN = 2;
    private const int DAILY_FREE_LEVELS_MAX_EXCLUSIVE = 4;
    [HideInInspector]
    public bool delayAnalyticsUntilGameplay;

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
    }

    void Start()
    {
        currentLevelIndex = SaveManager.instance.data.level;
        levelDatabase = Resources.Load<LevelDatabase>("LevelDatabase");

        LoadLevel(currentLevelIndex);
        Debug.Log("Saved Level: " + currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        Debug.Log("=== LOAD LEVEL CALLED ===");
        Debug.Log("Loading Level Index: " + index);

        currentLevelIndex = index;
        WorldData world = WorldManager.Instance.GetWorldForLevel(index + 1);

        if (world != null)
        {
            Debug.Log("World found: " + world.worldName);
            BackgroundManager.Instance.UpdateBackgrounds(world, index + 1);

            if (!skipMapRefresh)
            {
                MapManager.instance?.RefreshMap();
            }

            skipMapRefresh = false;
        }
        else
        {
            Debug.LogWarning("No world found for level: " + (index + 1));
        }

        if (index < levelDatabase.levels.Count)
        {
            LevelData handmadeLevel = levelDatabase.levels[index];
            ProceduralLevelData data = new ProceduralLevelData();
            data.layerLayouts = new List<string[]>();

            foreach (ShapeData shape in handmadeLevel.layers)
            {
                if (shape == null || shape.layout == null)
                    continue;

                string[] layoutCopy =
                    (string[])shape.layout.Clone();

                data.layerLayouts.Add(layoutCopy);
            }

            if (data.layerLayouts.Count == 0)
            {
                Debug.LogError(
                    $"Level {index} has no valid layouts."
                );
                return;
            }

            string[] biggestLayout = data.layerLayouts[0];
            data.layout = biggestLayout;
            data.rows = biggestLayout.Length;
            data.cols = biggestLayout[0].Length;

            data.stackStyle = handmadeLevel.stackStyle;
            data.stackOffsetX = handmadeLevel.stackOffsetX;
            data.stackOffsetY = handmadeLevel.stackOffsetY;
            data.difficulty = handmadeLevel.difficulty;

            BoardGenerator.instance.SetProceduralLevel(data);
            GameManager.instance.UpdateLevelText(index);
            Debug.Log("Loaded Handmade Level: " + index);
            return;
        }

        ProceduralLevelData levelData = ProceduralLevelGenerator.instance.GenerateLevel(index);

        if (levelData == null)
        {
            Debug.LogError("Level data is null");
            return;
        }

        BoardGenerator.instance.SetProceduralLevel(levelData);
        GameManager.instance.UpdateLevelText(index);
        Debug.Log("Loaded Infinite Level: " + index);
    }

    public void NextLevel(bool playParticle = true)
    {
        Debug.Log("=== NEXT LEVEL CALLED ===");

        if (isLoadingNextLevel)
        {
            Debug.LogWarning(
                "Already loading next level - ignoring call"
            );

            return;
        }

        // Lock immediately so double-tapping cannot launch
        // multiple ads or multiple level transitions.
        isLoadingNextLevel = true;

        if (nextLevelButton != null)
            nextLevelButton.interactable = false;

        bool shouldShowInterstitial =
            ShouldShowNextLevelInterstitial();

        if (shouldShowInterstitial &&
            AdManager.instance != null)
        {
            bool adStarted =
     AdManager.instance.ShowInterstitialAd(
         () =>
         {
             StartCoroutine(
                 NextLevelRoutine(playParticle)
             );
         },
         "next_level"
     );

            if (adStarted)
            {
                Debug.Log(
                    "Interstitial started before next level."
                );

                return;
            }

            Debug.Log(
                "Interstitial eligible but unavailable. " +
                "Continuing to next level."
            );
        }

        StartCoroutine(
            NextLevelRoutine(playParticle)
        );
    }

    private IEnumerator NextLevelRoutine(bool playParticle)
    {
        if (playParticle && nextLevelParticles != null)
        {
            nextLevelParticles.Play();
            yield return new WaitForSecondsRealtime(nextLevelParticleDuration);
        }

        currentLevelIndex++;
        Debug.Log("Moving to next level: " + currentLevelIndex);

        SaveManager.instance.data.level = currentLevelIndex;
        SaveManager.instance.SaveData();

        UIManager.Instance.HidePopup(ScreenType.LevelCompleted);
        LoadLevel(currentLevelIndex);

        if (!loadLevelSilently)
        {
            DOVirtual.DelayedCall(0.1f, () =>
            {
                if (BoardSpawner.instance != null)
                {
                    BoardSpawner.instance.PlaySpawnAnimation();
                }
            });
        }

        loadLevelSilently = false;

        GameManager.instance.ResetLevelState();

        // A normal next level begins immediately.
        // A destination/world transition waits until StartGame().
        if (!delayAnalyticsUntilGameplay)
        {
            GameManager.instance.BeginLevelAnalyticsSession();
        }

        delayAnalyticsUntilGameplay = false;

        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }

        isLoadingNextLevel = false;
    }

    public void UpdateNextButtonText()
    {
        int currentLevel = SaveManager.instance.data.level + 1;

        WorldData currentWorld = BackgroundManager.Instance.GetCurrentWorld();
        WorldData nextWorld = WorldManager.Instance.GetWorldForLevel(currentLevel + 1);

        bool worldChanging = nextWorld != currentWorld;
        bool unlockingDestination = BackgroundManager.Instance.IsNextDestinationUnlock();

        nextLevelText.text = (unlockingDestination || worldChanging) ? "Next Destination" : "Next Level";
    }

    private bool ShouldShowNextLevelInterstitial()
    {
        if (SaveManager.instance == null ||
            SaveManager.instance.data == null)
        {
            return false;
        }

        if (AdManager.instance == null)
        {
            return false;
        }

        if (AdManager.instance.IsAdsRemoved())
        {
            return false;
        }

        // This is the level that the player has just completed.
        int completedLevel =
            SaveManager.instance.data.level + 1;

        // No interstitial before completing Level 10.
        if (completedLevel < 10)
        {
            Debug.Log(
                $"Interstitial skipped: Level {completedLevel} " +
                "is below Level 10."
            );

            return false;
        }

        // Starting from Level 10, show every 2 levels:
        // 10, 12, 14, 16, 18...
        if (completedLevel % 2 != 0)
        {
            Debug.Log(
                $"Interstitial skipped: Level {completedLevel} " +
                "is not an interstitial level."
            );

            return false;
        }

        // IMPORTANT:
        // Keep your existing destination/world transition rule.
        if (IsDestinationOrWorldChanging())
        {
            Debug.Log(
                $"Interstitial skipped after Level {completedLevel}: " +
                "destination/world is changing."
            );

            return false;
        }

        Debug.Log(
            $"Interstitial eligible after Level {completedLevel}."
        );

        return true;
    }

    private void InitializeInterstitialPolicy(GameData data)
    {
        if (data.interstitialPolicyInitialized)
            return;

        data.interstitialPolicyInitialized = true;

        // Existing players do not incorrectly receive a new
        // 10-level installation grace period after updating the game.
        data.interstitialLifetimeNextClicks =
            Mathf.Max(0, data.level);

        SaveManager.instance.SaveData();
    }

    private void RefreshDailyInterstitialPolicy(GameData data)
    {
        string today = DateTime.Now.ToString(
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture
        );

        if (data.interstitialDailyDate == today)
            return;

        data.interstitialDailyDate = today;

        data.interstitialDailyFreeLevels =
            UnityEngine.Random.Range(
                DAILY_FREE_LEVELS_MIN,
                DAILY_FREE_LEVELS_MAX_EXCLUSIVE
            );

        data.interstitialDailyNextClicks = 0;

        SaveManager.instance.SaveData();

        Debug.Log(
            "New interstitial day. Free levels today: " +
            data.interstitialDailyFreeLevels
        );
    }

    private bool IsDestinationOrWorldChanging()
    {
        if (WorldManager.Instance == null ||
            BackgroundManager.Instance == null)
        {
            return false;
        }

        // SaveManager.data.level is zero-based.
        int currentDisplayLevel =
            SaveManager.instance.data.level + 1;

        WorldData currentWorld =
            BackgroundManager.Instance.GetCurrentWorld();

        WorldData nextWorld =
            WorldManager.Instance.GetWorldForLevel(
                currentDisplayLevel + 1
            );

        bool worldChanging =
            nextWorld != null &&
            nextWorld != currentWorld;

        bool destinationUnlocking =
            BackgroundManager.Instance
                .IsNextDestinationUnlock();

        return destinationUnlocking || worldChanging;
    }
}