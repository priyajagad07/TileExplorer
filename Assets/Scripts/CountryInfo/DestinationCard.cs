using UnityEngine;
using UnityEngine.UI;

public class DestinationCard : MonoBehaviour
{
    public CountryData country;
    public int destinationIndex;
    private bool isUnlocked;
    [SerializeField]
    private GameObject lockedMessagePopup;
    public void OnClick()
    {
        if (!isUnlocked)
        {
            MapScreenUI.instance.ShowLockedMessage(); return;
        }

        Debug.Log("CARD CLICKED");
        if (country == null)
            return;

        if (destinationIndex < 0 || destinationIndex >= country.destinations.Length)
        {
            Debug.LogError(
                "Invalid destination index: "
                + destinationIndex
                + " for "
                + country.countryName
            );

            return;
        }

        Debug.Log(country.countryName + " index=" + destinationIndex);

        CountryInfoScreen.instance.ShowDestination(
            country.destinations[destinationIndex]
        );

        CountryInfoScreen.instance.openedFromUnlock = false;
        Debug.Log("SHOWING SCREEN");

        UIManager.Instance.Show(ScreenType.CountryInfoScreen);
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
    }

}