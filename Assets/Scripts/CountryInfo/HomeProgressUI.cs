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

        // ---> THE FIX: Calculate the virtual level to support the infinite loop <---
        int virtualLevel = CountryManager.Instance.GetVirtualLevel(currentLevel);

        // 2. Fetch the current country data using the virtual level
        CountryData currentCountry = CountryManager.Instance.GetCountryForLevel(virtualLevel);

        if (currentCountry == null)
        {
            Debug.LogWarning("No country found for the current level.");
            return;
        }

        // 3. Determine the total number of destinations and levels
        int totalDestinations = currentCountry.destinations.Length;
        int countryLevels = currentCountry.endLevel - currentCountry.startLevel + 1;
        float levelsPerDestination = (float)countryLevels / totalDestinations;
        
        // ---> THE FIX: Use virtualLevel to see how many levels the player has beaten in THIS country <---
        int levelInsideCountry = virtualLevel - currentCountry.startLevel;

        // 4. Calculate exactly which destination the player is currently on (for the text)
        int currentIndex = Mathf.FloorToInt(levelInsideCountry / levelsPerDestination);
        currentIndex = Mathf.Clamp(currentIndex, 0, totalDestinations - 1);
        int displayValue = currentIndex + 1;

        // 5. Update the Slider (NOW INCREASES LEVEL-BY-LEVEL)
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = countryLevels;
            
            // The slider will fill up a tiny bit after every single level
            progressSlider.value = levelInsideCountry; 
        }

        // 6. Update the Text inside the circle (Still shows destination 1, 2, 3...)
        if (progressText != null)
        {
            progressText.text = displayValue.ToString();
        }
    }
}