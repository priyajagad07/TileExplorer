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

    [Header("Country Info Screen")]
    [SerializeField] private Image countryInfoScreenBackground;

    private CountryData currentCountry;

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

    public void UpdateBackgrounds(CountryData country, int playerLevel)
    {
        if (country == null)
            return;

        if (lastUpdatedLevel == playerLevel && currentCountry == country)
        {
            Debug.Log($"Background already updated for level {playerLevel}, skipping...");
            return;
        }

        currentCountry = country;
        lastUpdatedLevel = playerLevel;

        Sprite bg = GetBackgroundForLevel(country, playerLevel);

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

    public void SetCountryInfoScreen(Sprite bg)
    {
        if (countryInfoScreenBackground == null || bg == null) return;
        countryInfoScreenBackground.sprite = bg;
    }

    public CountryData GetCurrentCountry()
    {
        return currentCountry;
    }

    public void RefreshCurrentCountry()
    {
        if (currentCountry == null) return;

        int currentLevel = SaveManager.instance.data.level + 1;
        UpdateBackgrounds(currentCountry, currentLevel);
    }

    Sprite GetBackgroundForLevel(CountryData country, int playerLevel)
    {
        if (country.backgrounds == null || country.backgrounds.Length == 0) return null;

        int countryLevels = country.endLevel - country.startLevel + 1;
        float levelsPerDestination = (float)countryLevels / country.backgrounds.Length;

        int virtualLevel = CountryManager.Instance.GetVirtualLevel(playerLevel);
        int levelInsideCountry = virtualLevel - country.startLevel;

        int bgIndex = Mathf.FloorToInt(levelInsideCountry / levelsPerDestination);
        bgIndex = Mathf.Clamp(bgIndex, 0, country.backgrounds.Length - 1);

        return country.backgrounds[bgIndex];
    }

    public bool IsNextDestinationUnlock()
    {
        int currentLevel = SaveManager.instance.data.level + 1;

        int vCurrent = CountryManager.Instance.GetVirtualLevel(currentLevel);
        int vNext = CountryManager.Instance.GetVirtualLevel(currentLevel + 1);

        CountryData country = GetCurrentCountry();
        if (country == null) return false;

        CountryData nextCountry = CountryManager.Instance.GetCountryForLevel(currentLevel + 1);
        if (nextCountry != country) return true;

        int countryLevels = country.endLevel - country.startLevel + 1;
        float levelsPerDestination = (float)countryLevels / country.backgrounds.Length;

        int currentIndex = Mathf.FloorToInt((vCurrent - country.startLevel) / levelsPerDestination);
        int nextIndex = Mathf.FloorToInt((vNext - country.startLevel) / levelsPerDestination);

        return nextIndex > currentIndex;
    }

    public int GetNextDestinationIndex()
    {
        int currentLevel = SaveManager.instance.data.level + 1;

        int vNext = CountryManager.Instance.GetVirtualLevel(currentLevel + 1);

        CountryData currentCountry = GetCurrentCountry();
        if (currentCountry == null) return 0;

        CountryData nextCountry = CountryManager.Instance.GetCountryForLevel(currentLevel + 1);
        if (nextCountry != currentCountry) return 0;

        int countryLevels = currentCountry.endLevel - currentCountry.startLevel + 1;
        float levelsPerDestination = (float)countryLevels / currentCountry.backgrounds.Length;

        int nextIndex = Mathf.FloorToInt((vNext - currentCountry.startLevel) / levelsPerDestination);

        return Mathf.Clamp(nextIndex, 0, currentCountry.backgrounds.Length - 1);
    }
}