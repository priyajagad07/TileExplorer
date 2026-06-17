using UnityEngine;

public class CountryManager : MonoBehaviour
{
    public static CountryManager Instance;

    [SerializeField]
    private CountryDatabase countryDatabase;

    private void Awake()
    {
        Instance = this;
    }

    public CountryData GetCountryForLevel(int level)
    {
        foreach (CountryData country in countryDatabase.countries)
        {
            if (level >= country.startLevel &&
                level <= country.endLevel)
            {
                return country;
            }
        }

        return null;
    }

    public CountryDatabase GetDatabase()
    {
        return countryDatabase;
    }

    public bool IsCountryChanging()
    {
        int nextLevel =
            PlayerPrefs.GetInt("Level", 0) + 1;

        CountryData nextCountry =
            GetCountryForLevel(nextLevel);

        return nextCountry !=
               BackgroundManager.Instance.GetCurrentCountry();
    }

    public CountryData GetNextCountry()
    {
        int nextLevel =
            PlayerPrefs.GetInt("Level", 0) + 1;

        return GetCountryForLevel(nextLevel);
    }
}