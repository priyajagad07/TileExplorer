using UnityEngine;
using Solo.MOST_IN_ONE;
using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    private void Buy(string productId)
    {
        if (IAPManager.Instance == null)
        {
            Debug.LogWarning(
                "IAPManager is missing."
            );

            return;
        }

        bool started =
            IAPManager.Instance.BuyProduct(productId);

        if (!started)
        {
            Debug.LogWarning(
                "Purchase could not start: " +
                productId
            );
        }
    }

    // Coin packs
    public void BuySmallPack()
    {
        Buy(IAPProductIds.SmallPack);
    }

    public void BuyMediumPack()
    {
        Buy(IAPProductIds.MediumPack);
    }

    public void BuyLargePack()
    {
        Buy(IAPProductIds.LargePack);
    }

    public void BuyExtraLargePack()
    {
        Buy(IAPProductIds.ExtraLargePack);
    }

    public void BuyMegaPack()
    {
        Buy(IAPProductIds.MegaPack);
    }

    public void BuyUltimatePack()
    {
        Buy(IAPProductIds.UltimatePack);
    }

    // Bundles
    public void BuySuperBundle()
    {
        Buy(IAPProductIds.SuperBundle);
    }

    public void BuyMegaBundle()
    {
        Buy(IAPProductIds.MegaBundle);
    }

    public void BuyBrillianceBundle()
    {
        Buy(IAPProductIds.BrillianceBundle);
    }

    // Basic Remove Ads shop offer
    public void BuyRemoveAds()
    {
        Buy(IAPProductIds.RemoveAdsBasic);
    }
}