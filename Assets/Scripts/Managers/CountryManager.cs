using UnityEngine;

public class CountryManager : MonoBehaviour
{
    public static CountryManager Instance;

    [SerializeField]
    private CountryDatabase countryDatabase;

    [Header("Endgame Settings")]
    [Tooltip("If true, Level 301 loops back to the first country's background. If false, it stays on the final background forever.")]
    public bool loopCountriesInfinitely = true; 

    private void Awake()
    {
        Instance = this;
    }

    // --- THE NEW HELPER METHOD ---
    public int GetVirtualLevel(int realLevel)
    {
        if (countryDatabase == null || countryDatabase.countries.Count == 0) return realLevel;

        int maxLevel = 0;
        foreach (CountryData country in countryDatabase.countries)
        {
            if (country.endLevel > maxLevel)
            {
                maxLevel = country.endLevel;
            }
        }

        if (realLevel <= maxLevel) return realLevel;

        if (loopCountriesInfinitely)
        {
            // Wraps 301 back to 1, 302 to 2, etc.
            return ((realLevel - 1) % maxLevel) + 1;
        }
        else
        {
            // Caps the visual progression at 300
            return maxLevel; 
        }
    }

    public CountryData GetCountryForLevel(int level)
    {
        // Intercept the level before checking the database!
        int virtualLevel = GetVirtualLevel(level);

        foreach (CountryData country in countryDatabase.countries)
        {
            if (virtualLevel >= country.startLevel && virtualLevel <= country.endLevel)
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
        int nextLevel = SaveManager.instance.data.level + 1;
        CountryData nextCountry = GetCountryForLevel(nextLevel);
        return nextCountry != BackgroundManager.Instance.GetCurrentCountry();
    }

    public CountryData GetNextCountry()
    {
        int nextLevel = SaveManager.instance.data.level + 1;
        return GetCountryForLevel(nextLevel);
    }
}