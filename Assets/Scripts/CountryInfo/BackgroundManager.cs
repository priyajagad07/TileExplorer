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

    private int lastUpdatedLevel = -1;  // ← NEW!

    public void UpdateBackgrounds(CountryData country, int playerLevel)
    {
        if (country == null)
            return;

        // ← NEW! Prevent duplicate updates
        if (lastUpdatedLevel == playerLevel && currentCountry == country)
        {
            Debug.Log($"Background already updated for level {playerLevel}, skipping...");
            return;
        }

        currentCountry = country;
        lastUpdatedLevel = playerLevel;  // ← NEW! Track this level

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
        if (gameplayBackground == null || bg == null)
            return;

        gameplayBackground.sprite = bg;
    }

    public void SetHomeBackground(Sprite bg)
    {
        if (homeScreenBackground == null || bg == null)
            return;

        homeScreenBackground.sprite = bg;
    }

    public void SetDailyStreakBackground(Sprite bg)
    {
        if (dailyStreakScreenBackground == null || bg == null)
            return;

        dailyStreakScreenBackground.sprite = bg;
    }

    public void SetCountryInfoScreen(Sprite bg)
    {
        if (countryInfoScreenBackground == null || bg == null)
            return;

        countryInfoScreenBackground.sprite = bg;
    }

    public CountryData GetCurrentCountry()
    {
        return currentCountry;
    }

    public void RefreshCurrentCountry()
    {
        if (currentCountry == null)
            return;

        int currentLevel =
            PlayerPrefs.GetInt("Level", 0) + 1;

        UpdateBackgrounds(
            currentCountry,
            currentLevel
        );
    }

    Sprite GetBackgroundForLevel(CountryData country, int playerLevel)
    {
        if (
            country.backgrounds == null ||
            country.backgrounds.Length == 0
        )
        {
            return null;
        }

        int countryLevels =
            country.endLevel -
            country.startLevel + 1;

        int levelInsideCountry =
            playerLevel -
            country.startLevel;

        int levelsPerDestination =
     Mathf.CeilToInt(
         (float)countryLevels /
         country.backgrounds.Length
     );

        int bgIndex =
            levelInsideCountry /
            levelsPerDestination;

        bgIndex = Mathf.Clamp(
            bgIndex,
            0,
            country.backgrounds.Length - 1
        );

        return country.backgrounds[bgIndex];
    }


    public bool IsNextDestinationUnlock()
    {
        int currentLevel = PlayerPrefs.GetInt("Level", 0) + 1;

        CountryData country = GetCurrentCountry();

        if (country == null)
            return false;

        // COUNTRY CHANGE CHECK
        CountryData nextCountry =
            CountryManager.Instance.GetCountryForLevel(currentLevel + 1);

        if (nextCountry != country)
        {
            Debug.Log("Country changing!");
            return true;
        }

        int countryLevels =
            country.endLevel -
            country.startLevel + 1;

        int levelsPerDestination =
            Mathf.CeilToInt(
                (float)countryLevels /
                country.backgrounds.Length
            );

        int currentIndex =
            Mathf.Clamp(
                (currentLevel - country.startLevel) /
                levelsPerDestination,
                0,
                country.backgrounds.Length - 1
            );

        int nextIndex =
            Mathf.Clamp(
                (currentLevel + 1 - country.startLevel) /
                levelsPerDestination,
                0,
                country.backgrounds.Length - 1
            );

        Debug.Log(
            "CurrentLevel=" + currentLevel +
            " CurrentIndex=" + currentIndex +
            " NextIndex=" + nextIndex +
            " Country=" + country.countryName
        );

        return nextIndex > currentIndex;
    }


    public int GetNextDestinationIndex()
    {
        int currentLevel =
            PlayerPrefs.GetInt("Level", 0) + 1;

        CountryData currentCountry =
            GetCurrentCountry();

        if (currentCountry == null)
            return 0;

        CountryData nextCountry =
            CountryManager.Instance.GetCountryForLevel(currentLevel + 1);

        // COUNTRY CHANGE
        if (nextCountry != currentCountry)
        {
            return 0;
        }

        int countryLevels =
            currentCountry.endLevel -
            currentCountry.startLevel + 1;

        int levelsPerDestination =
            Mathf.CeilToInt(
                (float)countryLevels /
                currentCountry.backgrounds.Length
            );

        int nextIndex =
            Mathf.Clamp(
                (currentLevel + 1 - currentCountry.startLevel)
                / levelsPerDestination,
                0,
                currentCountry.backgrounds.Length - 1
            );

        return nextIndex;
    }
}