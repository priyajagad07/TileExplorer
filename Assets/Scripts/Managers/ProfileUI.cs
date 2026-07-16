using TMPro;
using UnityEngine;

public class ProfileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text countriesVisitedText;

    public static ProfileUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        coinsText.text =
            CoinManager.instance.GetCoins().ToString("N0");

        int currentLevel =
            SaveManager.instance.data.level + 1;

        CountryData currentCountry =
            CountryManager.Instance.GetCountryForLevel(currentLevel);

        if (currentCountry != null)
        {
            int visited =
                CountryManager.Instance
                .GetDatabase()
                .countries
                .IndexOf(currentCountry) + 1;

            countriesVisitedText.text =
                visited.ToString();
        }
    }
}