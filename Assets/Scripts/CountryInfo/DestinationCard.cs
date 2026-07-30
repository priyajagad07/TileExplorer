using UnityEngine;
using UnityEngine.UI;

public class DestinationCard : MonoBehaviour
{
    public WorldData world;
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

        if (world == null) return;

        if (destinationIndex < 0 || destinationIndex >= world.destinations.Length)
        {
            Debug.LogError("Invalid destination index: " + destinationIndex + " for " + world.worldName);
            return;
        }

        WorldInfoScreen.instance.openedFromUnlock = false;
        WorldInfoScreen.instance.openedFromMap = true;

        WorldInfoScreen.instance.ShowDestination(world.destinations[destinationIndex]);

        UIManager.Instance.Show(ScreenType.WorldInfoScreen);
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
    }
}