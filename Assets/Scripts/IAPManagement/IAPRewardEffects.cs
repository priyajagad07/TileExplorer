using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using Solo.MOST_IN_ONE;
using UnityEngine;

public class IAPRewardEffects : MonoBehaviour
{
    [Serializable]
    public class BundleParticleSet
    {
        [Tooltip("0 Coins, 1 Undo, 2 Magic, 3 Shuffle")]
        public List<UIParticle> particles =
            new List<UIParticle>();
    }

    [Header("Bundle Particles")]
    [SerializeField]
    private BundleParticleSet superBundleParticles;

    [SerializeField]
    private BundleParticleSet megaBundleParticles;

    [SerializeField]
    private BundleParticleSet brillianceBundleParticles;

    [SerializeField]
    private float bundleStepDelay = 0.5f;

    [Header("Coin Pack Particles")]
    [Tooltip(
        "0 Small, 1 Medium, 2 Large, " +
        "3 Extra Large, 4 Mega, 5 Ultimate"
    )]
    [SerializeField]
    private List<UIParticle> coinPackParticles =
        new List<UIParticle>();

    [Header("Remove Ads")]
    [SerializeField]
    private UIParticle basicRemoveAdsParticle;

    [SerializeField]
    private UIParticle deluxeRemoveAdsParticle;

    [Header("Timing")]
    [SerializeField]
    private float rewardSoundDelay = 0.8f;

    [Header("Particle Arrival")]
    [SerializeField]
    private float particleArrivalDelay = 0.8f;

    private void OnEnable()
    {
        PurchaseManager.PurchaseGranted +=
            HandlePurchaseGranted;
    }

    private void OnDisable()
    {
        PurchaseManager.PurchaseGranted -=
            HandlePurchaseGranted;

        DOTween.Kill(this);
    }

    private void HandlePurchaseGranted(string productId)
    {
        switch (productId)
        {
            // Coin packs
            case IAPProductIds.SmallPack:
                PlayCoinPackEffect(0);
                break;

            case IAPProductIds.MediumPack:
                PlayCoinPackEffect(1);
                break;

            case IAPProductIds.LargePack:
                PlayCoinPackEffect(2);
                break;

            case IAPProductIds.ExtraLargePack:
                PlayCoinPackEffect(3);
                break;

            case IAPProductIds.MegaPack:
                PlayCoinPackEffect(4);
                break;

            case IAPProductIds.UltimatePack:
                PlayCoinPackEffect(5);
                break;

            // Each bundle uses its own four particles.
            case IAPProductIds.SuperBundle:
                PlayBundleEffects(
                    superBundleParticles
                );
                break;

            case IAPProductIds.MegaBundle:
                PlayBundleEffects(
                    megaBundleParticles
                );
                break;

            case IAPProductIds.BrillianceBundle:
                PlayBundleEffects(
                    brillianceBundleParticles
                );
                break;

            case IAPProductIds.RemoveAdsBasic:
                PlayRemoveAdsEffect(
                    basicRemoveAdsParticle,
                    false
                );
                break;

            case IAPProductIds.RemoveAdsDeluxe:
                PlayRemoveAdsEffect(
                    deluxeRemoveAdsParticle,
                    true
                );
                break;
        }
    }

    private void PlayCoinPackEffect(int index)
    {
        PlayStartFeedback();

        UIParticle particle =
            GetParticle(
                coinPackParticles,
                index
            );

        particle?.Play();

        DOVirtual.DelayedCall(
            particleArrivalDelay,
            () =>
            {
                CoinManager.instance
                    ?.RefreshCoinsUI();

                SoundManager.instance?.PlaySound(
                    SoundName.CoinReach
                );
            }
        ).SetId(this);
    }

    private void PlayBundleEffects(
    BundleParticleSet bundleSet)
    {
        if (bundleSet == null ||
            bundleSet.particles == null)
        {
            Debug.LogWarning(
                "Bundle particle set is missing."
            );

            return;
        }

        PlayStartFeedback();

        for (int i = 0;
             i < bundleSet.particles.Count;
             i++)
        {
            int particleIndex = i;

            float startDelay =
                particleIndex * bundleStepDelay;

            DOVirtual.DelayedCall(
                startDelay,
                () =>
                {
                    UIParticle particle =
                        GetParticle(
                            bundleSet.particles,
                            particleIndex
                        );

                    particle?.Play();

                    DOVirtual.DelayedCall(
                        particleArrivalDelay,
                        () =>
                        {
                            RefreshBundleText(
                                particleIndex
                            );

                            SoundManager.instance
                                ?.PlaySound(
                                    SoundName.CoinReach
                                );
                        }
                    ).SetId(this);
                }
            ).SetId(this);
        }
    }

    private void RefreshBundleText(
    int particleIndex)
    {
        // Inspector order:
        // 0 Coins
        // 1 Undo
        // 2 Magic
        // 3 Shuffle

        switch (particleIndex)
        {
            case 0:
                CoinManager.instance
                    ?.RefreshCoinsUI();
                break;

            case 1:
                BoosterManager.instance
                    ?.RefreshUndoUI();
                break;

            case 2:
                BoosterManager.instance
                    ?.RefreshMagicUI();
                break;

            case 3:
                BoosterManager.instance
                    ?.RefreshShuffleUI();
                break;
        }
    }

    private void PlayRemoveAdsEffect(
    UIParticle particle,
    bool includesBoosters)
    {
        PlayStartFeedback();

        particle?.Play();

        DOVirtual.DelayedCall(
            particleArrivalDelay,
            () =>
            {
                CoinManager.instance
                    ?.RefreshCoinsUI();

                if (includesBoosters)
                {
                    BoosterManager.instance
                        ?.UpdateUI();
                }

                SoundManager.instance?.PlaySound(
                    SoundName.CoinReach
                );
            }
        ).SetId(this);
    }

    private void PlayStartFeedback()
    {
        if (SoundManager.instance == null)
            return;

        SoundManager.instance.PlaySound(
            SoundName.Coins
        );

        SoundManager.instance.PlayHaptic(
            MOST_HapticFeedback
                .HapticTypes.Success
        );
    }

    private void PlayRewardSoundAfterDelay(
        float delay)
    {
        DOVirtual.DelayedCall(
            delay,
            () =>
            {
                SoundManager.instance?.PlaySound(
                    SoundName.CoinReach
                );
            }
        ).SetId(this);
    }

    private UIParticle GetParticle(
        List<UIParticle> particles,
        int index)
    {
        if (particles == null ||
            index < 0 ||
            index >= particles.Count)
        {
            Debug.LogWarning(
                "Missing IAP particle at index: " +
                index
            );

            return null;
        }

        return particles[index];
    }
}