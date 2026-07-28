using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    [Header("Gameplay Screen")]
    [SerializeField] private Image gameplayBackground;

    [Header("Home Screen")]
    [SerializeField] private Image homeScreenBackground;

    [Header("Map Screen")]
    [SerializeField] private Image mapBackground;

    [Header("Daily Streak Screen")]
    [SerializeField] private Image dailyStreakScreenBackground;

    [Header("World Info Screen")]
    [SerializeField] private Image worldInfoScreenBackground;

    private WorldData currentWorld;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private int lastUpdatedLevel = -1;

    public void UpdateBackgrounds(WorldData world, int playerLevel)
    {
        if (world == null)
            return;

        if (lastUpdatedLevel == playerLevel && currentWorld == world)
        {
            Debug.Log($"Background already updated for level {playerLevel}, skipping...");
            return;
        }

        currentWorld = world;
        lastUpdatedLevel = playerLevel;

        Sprite bg = GetBackgroundForLevel(world, playerLevel);

        SetGameplayBackground(bg);
        SetHomeBackground(bg);
        SetDailyStreakBackground(bg);

        if (mapBackground != null)
        {
            mapBackground.sprite = bg;
        }
    }

    public void SetGameplayBackground(Sprite bg)
    {
        if (gameplayBackground == null || bg == null) return;
        gameplayBackground.sprite = bg;
    }

    public void SetHomeBackground(Sprite bg)
    {
        if (homeScreenBackground == null || bg == null) return;
        homeScreenBackground.sprite = bg;
    }

    public void SetDailyStreakBackground(Sprite bg)
    {
        if (dailyStreakScreenBackground == null || bg == null) return;
        dailyStreakScreenBackground.sprite = bg;
    }

    public void SetWorldInfoScreen(Sprite bg)
    {
        if (worldInfoScreenBackground == null || bg == null) return;
        worldInfoScreenBackground.sprite = bg;
    }

    public WorldData GetCurrentWorld()
    {
        return currentWorld;
    }

    public void RefreshCurrentWorld()
    {
        if (currentWorld == null) return;

        int currentLevel = SaveManager.instance.data.level + 1;
        UpdateBackgrounds(currentWorld, currentLevel);
    }

    Sprite GetBackgroundForLevel(WorldData world, int playerLevel)
    {
        if (world.backgrounds == null || world.backgrounds.Length == 0) return null;

        int worldLevels = world.endLevel - world.startLevel + 1;
        float levelsPerDestination = (float)worldLevels / world.backgrounds.Length;

        int virtualLevel = WorldManager.Instance.GetVirtualLevel(playerLevel);
        int levelInsideWorld = virtualLevel - world.startLevel;

        int bgIndex = Mathf.FloorToInt(levelInsideWorld / levelsPerDestination);
        bgIndex = Mathf.Clamp(bgIndex, 0, world.backgrounds.Length - 1);

        return world.backgrounds[bgIndex];
    }

    public bool IsNextDestinationUnlock()
    {
        int currentLevel = SaveManager.instance.data.level + 1;

        int vCurrent = WorldManager.Instance.GetVirtualLevel(currentLevel);
        int vNext = WorldManager.Instance.GetVirtualLevel(currentLevel + 1);

        WorldData world = GetCurrentWorld();
        if (world == null) return false;

        WorldData nextWorld = WorldManager.Instance.GetWorldForLevel(currentLevel + 1);
        if (nextWorld != world) return true;

        int worldLevels = world.endLevel - world.startLevel + 1;
        float levelsPerDestination = (float)worldLevels / world.backgrounds.Length;

        int currentIndex = Mathf.FloorToInt((vCurrent - world.startLevel) / levelsPerDestination);
        int nextIndex = Mathf.FloorToInt((vNext - world.startLevel) / levelsPerDestination);

        return nextIndex > currentIndex;
    }

    public int GetNextDestinationIndex()
    {
        int currentLevel = SaveManager.instance.data.level + 1;

        int vNext = WorldManager.Instance.GetVirtualLevel(currentLevel + 1);

        WorldData currentWorld = GetCurrentWorld();
        if (currentWorld == null) return 0;

        WorldData nextWorld = WorldManager.Instance.GetWorldForLevel(currentLevel + 1);
        if (nextWorld != currentWorld) return 0;

        int worldLevels = currentWorld.endLevel - currentWorld.startLevel + 1;
        float levelsPerDestination = (float)worldLevels / currentWorld.backgrounds.Length;

        int nextIndex = Mathf.FloorToInt((vNext - currentWorld.startLevel) / levelsPerDestination);

        return Mathf.Clamp(nextIndex, 0, currentWorld.backgrounds.Length - 1);
    }
}