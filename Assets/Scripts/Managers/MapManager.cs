using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("Lock Sprite")]
    public Sprite lockedCardSprite;

    [Header("Country Panels")]
    [Tooltip("Drag all your CountryUIPanel objects (France, Italy, etc.) into this list")]
    public List<CountryUIPanel> countryPanels;
    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        RefreshMap();
    }

    public void RefreshMap()
    {
        if (CountryManager.Instance == null) return;

        int currentLevel = PlayerPrefs.GetInt("Level", 0) + 1;
        UpdateAllCountries(currentLevel);
    }

    private void UpdateAllCountries(int currentLevel)
    {
        CountryDatabase database = CountryManager.Instance.GetDatabase();
        if (database == null) return;

        foreach (CountryData country in database.countries)
        {
            // Safely find the matching panel from our list instead of searching the whole scene by string
            CountryUIPanel matchingPanel = countryPanels.Find(p => p.countryData == country);

            if (matchingPanel == null) continue;

            UpdateCountryCards(matchingPanel, country, currentLevel);
        }
    }

    private void UpdateCountryCards(CountryUIPanel panel, CountryData country, int currentLevel)
    {
        int totalLevels = country.endLevel - country.startLevel + 1;
        int totalCards = country.previewCards.Length;

        // Exact same float division as BackgroundManager
        float levelsPerCard = (float)totalLevels / totalCards;
        int unlockedCards = 0;

        if (currentLevel >= country.startLevel)
        {
            int levelInsideCountry = currentLevel - country.startLevel;

            // Exact same FloorToInt math
            int currentIndex = Mathf.FloorToInt(levelInsideCountry / levelsPerCard);

            // The number of unlocked cards is just the index + 1
            unlockedCards = currentIndex + 1;
        }

        unlockedCards = Mathf.Clamp(unlockedCards, 0, totalCards);

        for (int i = 0; i < totalCards; i++)
        {
            if (i >= panel.destinationCards.Length) continue;

            DestinationCard destinationCard = panel.destinationCards[i];
            if (destinationCard == null) continue;

            destinationCard.country = country;
            destinationCard.destinationIndex = i;

            bool unlocked = i < unlockedCards;
            destinationCard.SetUnlocked(unlocked);

            if (destinationCard.cityImage != null)
            {
                destinationCard.cityImage.sprite = unlocked ? country.previewCards[i] : lockedCardSprite;
            }
        }
    }
}