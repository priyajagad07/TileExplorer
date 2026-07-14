using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("Lock Sprite")]
    public Sprite lockedCardSprite;

    [Header("Country Panels")]
    public List<CountryUIPanel> countryPanels;
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

    private void Start()
    {
        RefreshMap();
    }

    public void RefreshMap()
    {
        if (CountryManager.Instance == null) return;

        int currentLevel = SaveManager.instance.data.level + 1;
        UpdateAllCountries(currentLevel);
    }

   private void UpdateAllCountries(int currentLevel)
    {
        CountryDatabase database = CountryManager.Instance.GetDatabase();
        if (database == null) return;

        int virtualLevel = CountryManager.Instance.GetVirtualLevel(currentLevel);

        foreach (CountryData country in database.countries)
        {
            CountryUIPanel matchingPanel = countryPanels.Find(p => p.countryData == country);

            if (matchingPanel == null) continue;

            UpdateCountryCards(matchingPanel, country, virtualLevel); // Pass virtualLevel here!
        }
    }

    private void UpdateCountryCards(CountryUIPanel panel, CountryData country, int currentLevel)
    {
        int totalLevels = country.endLevel - country.startLevel + 1;
        int totalCards = country.previewCards.Length;

        float levelsPerCard = (float)totalLevels / totalCards;
        int unlockedCards = 0;

        if (currentLevel >= country.startLevel)
        {
            int levelInsideCountry = currentLevel - country.startLevel;
            int currentIndex = Mathf.FloorToInt(levelInsideCountry / levelsPerCard);
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