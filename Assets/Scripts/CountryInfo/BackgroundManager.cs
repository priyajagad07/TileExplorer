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

    public void UpdateBackgrounds(CountryData country, int playerLevel)
    {
        if (country == null)
            return;

        currentCountry = country;

        Sprite bg =
            GetBackgroundForLevel(
                country,
                playerLevel
            );

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

        float progress =
    (float)levelInsideCountry /
    (countryLevels - 1);

        int bgIndex =
            Mathf.FloorToInt(
                progress *
                country.backgrounds.Length
            );

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

        int countryLevels =
            country.endLevel -
            country.startLevel + 1;

        float currentProgress =
            (float)(currentLevel - country.startLevel) /
            (countryLevels - 1);

        int currentIndex =
            Mathf.FloorToInt(
                currentProgress *
                country.backgrounds.Length
            );

        int nextLevel = currentLevel + 1;

        float nextProgress =
            (float)(nextLevel - country.startLevel) /
            (countryLevels - 1);

        int nextIndex =
            Mathf.FloorToInt(
                nextProgress *
                country.backgrounds.Length
            );

        return nextIndex > currentIndex;
    }

    public int GetNextDestinationIndex()
    {
        int currentLevel =
            PlayerPrefs.GetInt(
                "Level",
                0
            ) + 1;

        CountryData country =
            GetCurrentCountry();

        if (country == null)
            return 0;

        int countryLevels =
            country.endLevel -
            country.startLevel + 1;

        float nextProgress =
            (float)(
                currentLevel + 1 -
                country.startLevel
            )
            /
            (countryLevels - 1);

        int nextIndex =
            Mathf.FloorToInt(
                nextProgress *
                country.backgrounds.Length
            );

        return Mathf.Clamp(
            nextIndex,
            0,
            country.backgrounds.Length - 1
        );
    }
}