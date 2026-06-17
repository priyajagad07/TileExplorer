using UnityEngine;
using UnityEngine.UI;

public class DestinationCard : MonoBehaviour
{
    public CountryData country;
    public int destinationIndex;
    private bool isUnlocked;
    
    [SerializeField]
    private GameObject lockedMessagePopup;

    [Header("UI References")]
    [Tooltip("Drag the 'City - Image' child object here")]
    public Image cityImage;

    public void OnClick()
    {
        if (!isUnlocked)
        {
            MapScreenUI.instance.ShowLockedMessage(); 
            return;
        }

        if (country == null) return;

        if (destinationIndex < 0 || destinationIndex >= country.destinations.Length)
        {
            Debug.LogError("Invalid destination index: " + destinationIndex + " for " + country.countryName);
            return;
        }

        CountryInfoScreen.instance.ShowDestination(country.destinations[destinationIndex]);
        CountryInfoScreen.instance.openedFromUnlock = false;
        UIManager.Instance.Show(ScreenType.CountryInfoScreen);
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
    }
}