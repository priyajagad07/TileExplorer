using System.Collections.Generic;
using UnityEngine.Purchasing;

public static class IAPProductIds
{
    // ==========================================
    // COIN PACKS
    // ==========================================

    public const string SmallPack =
        "com.taptileconnect.smallpack";

    public const string MediumPack =
        "com.taptileconnect.mediumpack";

    public const string LargePack =
        "com.taptileconnect.largepack";

    public const string ExtraLargePack =
        "com.taptileconnect.extralargepack";

    public const string MegaPack =
        "com.taptileconnect.megapack";

    public const string UltimatePack =
        "com.taptileconnect.ultimatepack";

    // ==========================================
    // BUNDLES
    // ==========================================

    public const string SuperBundle =
        "com.taptileconnect.superbundle";

    public const string MegaBundle =
        "com.taptileconnect.megabundle";

    public const string BrillianceBundle =
        "com.taptileconnect.brilliancebundle";

    // ==========================================
    // REMOVE ADS
    // ==========================================

    public const string RemoveAdsBasic =
        "com.taptileconnect.removeadsbasic";

    public const string RemoveAdsDeluxe =
        "com.taptileconnect.removeadsdeluxe";

    public static readonly List<ProductDefinition> Definitions =
        new List<ProductDefinition>
        {
            new ProductDefinition(
                SmallPack,
                ProductType.Consumable
            ),
            new ProductDefinition(
                MediumPack,
                ProductType.Consumable
            ),
            new ProductDefinition(
                LargePack,
                ProductType.Consumable
            ),
            new ProductDefinition(
                ExtraLargePack,
                ProductType.Consumable
            ),
            new ProductDefinition(
                MegaPack,
                ProductType.Consumable
            ),
            new ProductDefinition(
                UltimatePack,
                ProductType.Consumable
            ),

            new ProductDefinition(
                SuperBundle,
                ProductType.Consumable
            ),
            new ProductDefinition(
                MegaBundle,
                ProductType.Consumable
            ),
            new ProductDefinition(
                BrillianceBundle,
                ProductType.Consumable
            ),

            new ProductDefinition(
                RemoveAdsBasic,
                ProductType.NonConsumable
            ),
            new ProductDefinition(
                RemoveAdsDeluxe,
                ProductType.NonConsumable
            )
        };
}