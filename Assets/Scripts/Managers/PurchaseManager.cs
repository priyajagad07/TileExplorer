using System.Collections.Generic;
using UnityEngine;
using System;

public class PurchaseManager : MonoBehaviour
{
    public static PurchaseManager instance;

    [Header("Remove Ads UI")]
    [Tooltip(
        "Add both Remove Ads offer objects here."
    )]
    [SerializeField]
    private List<GameObject> removeAdsOffers = new List<GameObject>();
    public static event Action<string> PurchaseGranted;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public bool IsRemoveAdsPurchased()
    {
        return SaveManager.instance != null &&
               SaveManager.instance.data != null &&
               SaveManager.instance.data
                   .removeAdsPurchased == 1;
    }

    // Keep this for the Deluxe popup Buy button.
    public void PurchaseRemoveAds()
    {
        IAPManager.Instance?.BuyProduct(
            IAPProductIds.RemoveAdsDeluxe
        );
    }

    // Use this for the shop-screen basic offer.
    public void PurchaseBasicRemoveAds()
    {
        IAPManager.Instance?.BuyProduct(
            IAPProductIds.RemoveAdsBasic
        );
    }

    public bool GrantNewPurchase(string productId)
    {
        switch (productId)
        {
            // Coin packs
            case IAPProductIds.SmallPack:
                CoinManager.instance.AddCoins(240, false, productId);
                break;

            case IAPProductIds.MediumPack:
                CoinManager.instance.AddCoins(720, false, productId);
                break;

            case IAPProductIds.LargePack:
                CoinManager.instance.AddCoins(1500, false, productId);
                break;

            case IAPProductIds.ExtraLargePack:
                CoinManager.instance.AddCoins(3200, false, productId);
                break;

            case IAPProductIds.MegaPack:
                CoinManager.instance.AddCoins(6600, false, productId);
                break;

            case IAPProductIds.UltimatePack:
                CoinManager.instance.AddCoins(15000, false, productId);
                break;

            // Bundles
            case IAPProductIds.SuperBundle:
                GrantBundle(
                    productId,
                    coins: 1200,
                    undo: 3,
                    shuffle: 3,
                    magic: 3
                );
                break;

            case IAPProductIds.MegaBundle:
                GrantBundle(
                    productId,
                    coins: 3000,
                    undo: 6,
                    shuffle: 6,
                    magic: 6
                );
                break;

            case IAPProductIds.BrillianceBundle:
                GrantBundle(
                    productId,
                    coins: 6800,
                    undo: 12,
                    shuffle: 12,
                    magic: 12
                );
                break;

            // Remove Ads
            case IAPProductIds.RemoveAdsBasic:
                GrantRemoveAdsOffer(
                    source: productId,
                    coins: 300,
                    undo: 0,
                    shuffle: 0,
                    magic: 0
                );
                break;

            case IAPProductIds.RemoveAdsDeluxe:
                GrantRemoveAdsOffer(
                    source: productId,
                    coins: 5000,
                    undo: 4,
                    shuffle: 5,
                    magic: 3
                );
                break;

            default:
                Debug.LogError(
                    "No reward configured for: " +
                    productId
                );

                return false;
        }

        SaveManager.instance.SaveData();

        // CoinsUI.RefreshAll();
        // BoosterManager.instance?.UpdateUI();

        // Tell visual effects that a real purchase succeeded.
        PurchaseGranted?.Invoke(productId);

        RefreshUI();

        Debug.Log("IAP reward granted: " + productId);

        return true;
    }

    public bool RestoreNonConsumable(
    string productId)
    {
        if (!IsRemoveAdsProduct(productId))
            return false;

        bool wasAlreadyEnabled =
            IsRemoveAdsPurchased();

        EnableRemoveAds();

        SaveManager.instance.SaveData();

        RefreshUI();

        if (!wasAlreadyEnabled)
        {
            Debug.Log(
                "Remove Ads entitlement restored."
            );

            return true;
        }

        return false;
    }

    private void GrantBundle(
    string source,
    int coins,
    int undo,
    int shuffle,
    int magic)
    {
        CoinManager.instance.AddCoins(
            coins,
            false,
            source
        );

        BoosterManager.instance.AddBoosters(
            undo,
            shuffle,
            magic,
            false,
            source
        );
    }

    private void GrantRemoveAdsOffer(
    string source,
    int coins,
    int undo,
    int shuffle,
    int magic)
    {
        bool alreadyPurchased =
            IsRemoveAdsPurchased();

        EnableRemoveAds();

        // Do not grant the bonus from another Remove Ads
        // product if Remove Ads is already owned.
        if (alreadyPurchased)
            return;

        if (coins > 0)
        {
            CoinManager.instance.AddCoins(
                coins,
                false,
                source
            );
        }

        if (undo > 0 ||
            shuffle > 0 ||
            magic > 0)
        {
            BoosterManager.instance.AddBoosters(
                undo,
                shuffle,
                magic,
                false,
                source
            );
        }
    }

    private void EnableRemoveAds()
    {
        SaveManager.instance.data.removeAdsPurchased = 1;
        AdManager.instance?.DisableNonRewardedAds();
    }

    private bool IsRemoveAdsProduct(
        string productId)
    {
        return productId ==
                   IAPProductIds.RemoveAdsBasic ||
               productId ==
                   IAPProductIds.RemoveAdsDeluxe;
    }

    public void RefreshUI()
    {
        bool purchased = IsRemoveAdsPurchased();

        foreach (GameObject offer in removeAdsOffers)
        {
            if (offer != null)
            {
                offer.SetActive(!purchased);
            }
        }

        if (purchased && UIManager.Instance != null)
        {
            UIManager.Instance.HidePopup(
                ScreenType.RemoveAdsPopup
            );
        }
    }
}