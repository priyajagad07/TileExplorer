using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("Lock Sprite")]
    public Sprite lockedCardSprite;

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
        Debug.Log("RefreshMap Level = " + (PlayerPrefs.GetInt("Level", 0) + 1));

        if (CountryManager.Instance == null)
        {
            Debug.LogError("CountryManager Instance NULL");
            return;
        }

        int currentLevel = PlayerPrefs.GetInt("Level", 0) + 1;
        UpdateAllCountries(currentLevel);
    }

    private void UpdateAllCountries(int currentLevel)
    {
        CountryDatabase database = CountryManager.Instance.GetDatabase();

        if (database == null)
        {
            return;
        }

        foreach (CountryData country in database.countries)
        {
            Transform countryTransform = GameObject.Find(country.countryName)?.transform;

            if (countryTransform == null)
                continue;

            UpdateCountryCards(
                countryTransform,
                country,
                currentLevel
            );
        }
    }

    private void UpdateCountryCards(Transform countryTransform, CountryData country, int currentLevel)
    {
        int totalLevels = country.endLevel - country.startLevel + 1;

        int totalCards = country.previewCards.Length;

        float levelsPerCard = (float)totalLevels / totalCards;

        int unlockedCards = 0;

        if (currentLevel >= country.startLevel)
        {
            int progress = Mathf.Clamp(currentLevel - country.startLevel + 1, 0, totalLevels);

            unlockedCards = Mathf.CeilToInt(progress / levelsPerCard);
        }

        Debug.Log(country.countryName + " unlockedCards=" + unlockedCards);

        unlockedCards = Mathf.Clamp(unlockedCards, 0, totalCards);

        for (int i = 0; i < totalCards; i++)
        {
            Transform card = countryTransform.Find("Card" + (i + 1) + " - Button");

            if (card == null)
            {
                continue;
            }

            DestinationCard destinationCard = card.GetComponent<DestinationCard>();

            if (destinationCard != null)
            {
                destinationCard.SetUnlocked(
                    i < unlockedCards
                );
            }

            if (destinationCard != null)
            {
                destinationCard.country = country;
                destinationCard.destinationIndex = i;
            }

            Transform cityImage =
                card.Find("City - Image");

            if (cityImage == null)
                continue;

            Image image =
                cityImage.GetComponent<Image>();

            if (image == null)
                continue;

            bool unlocked = i < unlockedCards;

            if (unlocked)
            {
                image.sprite =
                    country.previewCards[i];
            }
            else
            {
                image.sprite =
                    lockedCardSprite;
            }

            if (destinationCard != null)
            {
                destinationCard.SetUnlocked(
                    unlocked
                );
            }
        }
    }
}