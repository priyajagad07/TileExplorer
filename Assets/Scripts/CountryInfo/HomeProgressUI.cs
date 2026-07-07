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
        // Start runs after all Awakes, so managers are guaranteed to be ready here
        UpdateProgressUI();
    }

    private void OnEnable()
    {
        // Only update if managers are already initialized (prevents crash on first frame)
        if (CountryManager.Instance != null && SaveManager.instance != null)
        {
            UpdateProgressUI();
        }
    }

    public void UpdateProgressUI()
    {
        // 1. Get the current player level
        int currentLevel = SaveManager.instance.data.level + 1;

        // 2. Fetch the current country data
        CountryData currentCountry = CountryManager.Instance.GetCountryForLevel(currentLevel);

        if (currentCountry == null)
        {
            Debug.LogWarning("No country found for the current level.");
            return;
        }

        // 3. Determine the total number of destinations
        int totalDestinations = currentCountry.destinations.Length;

        // 4. Calculate exactly which destination the player is currently on
        int countryLevels = currentCountry.endLevel - currentCountry.startLevel + 1;
        float levelsPerDestination = (float)countryLevels / totalDestinations;
        int levelInsideCountry = currentLevel - currentCountry.startLevel;

        int currentIndex = Mathf.FloorToInt(levelInsideCountry / levelsPerDestination);

        // Clamp it to prevent index out of bounds errors
        currentIndex = Mathf.Clamp(currentIndex, 0, totalDestinations - 1);

        // 5. Convert to a 1-based number for the user interface
        int displayValue = currentIndex + 1;

        // 6. Update the Slider (Now starting at 1)
        if (progressSlider != null)
        {
            progressSlider.minValue = 1;
            progressSlider.maxValue = totalDestinations;
            progressSlider.value = displayValue;
        }

        // 7. Update the Text inside the circle
        if (progressText != null)
        {
            progressText.text = displayValue.ToString();
        }
    }
}