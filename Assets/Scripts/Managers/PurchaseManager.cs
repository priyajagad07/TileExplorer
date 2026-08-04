using UnityEngine;

public class PurchaseManager : MonoBehaviour
{
    public static PurchaseManager instance;

    [Header("Remove Ads UI")]
    [SerializeField] private GameObject removeAdsButton;
    [SerializeField] private GameObject removeAdsPopup;

    private const int REMOVE_ADS_COINS = 5000;
    private const int REMOVE_ADS_UNDO = 4;
    private const int REMOVE_ADS_SHUFFLE = 5;
    private const int REMOVE_ADS_MAGIC = 3;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        RefreshUI();
    }

    public bool IsRemoveAdsPurchased()
    {
        return SaveManager.instance.data.removeAdsPurchased == 1;
    }

    // Assign this to your Buy button
    public void PurchaseRemoveAds()
    {
        if (IsRemoveAdsPurchased())
            return;

        GrantRemoveAdsRewards();
    }

    private void GrantRemoveAdsRewards()
    {
        if (IsRemoveAdsPurchased())
            return;

        SaveManager.instance.data.removeAdsPurchased = 1;

        CoinManager.instance.AddCoins(REMOVE_ADS_COINS);

        BoosterManager.instance.AddBoosters(
            REMOVE_ADS_UNDO,
            REMOVE_ADS_SHUFFLE,
            REMOVE_ADS_MAGIC
        );

        // Save after all purchase rewards have been applied.
        SaveManager.instance.SaveData();

        // Immediately remove any banner or loaded interstitial.
        AdManager.instance?.DisableNonRewardedAds();

        CoinsUI.RefreshAll();
        RefreshUI();

        if (removeAdsPopup != null)
            removeAdsPopup.SetActive(false);

        Debug.Log("Remove Ads Purchased Successfully");
    }
    
    public void RefreshUI()
    {
        bool purchased = IsRemoveAdsPurchased();

        if (removeAdsButton != null)
            removeAdsButton.SetActive(!purchased);
    }
}