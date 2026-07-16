using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    private static readonly List<ProgressUI> instances = new();

    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text levelText;

    private void Awake()
    {
        instances.Add(this);
    }

    private void OnDestroy()
    {
        instances.Remove(this);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (SaveManager.instance == null || CountryManager.Instance == null)
            return;

        int currentLevel = SaveManager.instance.data.level + 1;
        int virtualLevel = CountryManager.Instance.GetVirtualLevel(currentLevel);

        CountryData country =
            CountryManager.Instance.GetCountryForLevel(virtualLevel);

        if (country == null)
            return;

        int totalDestinations = country.destinations.Length;
        int countryLevels = country.endLevel - country.startLevel + 1;
        float levelsPerDestination = (float)countryLevels / totalDestinations;

        int levelInsideCountry = virtualLevel - country.startLevel;
        int completed = Mathf.Max(levelInsideCountry - 1, 0);

        int destination =
            Mathf.Clamp(
                Mathf.FloorToInt(completed / levelsPerDestination),
                0,
                totalDestinations - 1);

        slider.minValue = 0;
        slider.maxValue = totalDestinations;
        slider.value = destination + 1;

        levelText.text = (destination + 1).ToString();
    }

    public static void RefreshAll()
    {
        foreach (var ui in instances)
            ui.Refresh();
    }
}