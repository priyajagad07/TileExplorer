using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeProgressUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;

    private void Start()
    {
        UpdateProgressUI();
    }

    private void OnEnable()
    {
        if (CountryManager.Instance != null && SaveManager.instance != null)
        {
            UpdateProgressUI();
        }
    }

   public void UpdateProgressUI()
    {
        int currentLevel = SaveManager.instance.data.level + 1;
        int virtualLevel = CountryManager.Instance.GetVirtualLevel(currentLevel);

        CountryData currentCountry = CountryManager.Instance.GetCountryForLevel(virtualLevel);

        if (currentCountry == null)
        {
            Debug.LogWarning("No country found for the current level.");
            return;
        }

        int totalDestinations = currentCountry.destinations.Length;
        int countryLevels = currentCountry.endLevel - currentCountry.startLevel + 1;
        float levelsPerDestination = (float)countryLevels / totalDestinations;
        int levelInsideCountry = virtualLevel - currentCountry.startLevel;

        int currentIndex = Mathf.FloorToInt(levelInsideCountry / levelsPerDestination);
        currentIndex = Mathf.Clamp(currentIndex, 0, totalDestinations - 1);
        int displayValue = currentIndex + 1;

        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = countryLevels;
            progressSlider.value = levelInsideCountry; 
        }
        
        if (progressText != null)
        {
            progressText.text = displayValue.ToString();
        }
    }
}