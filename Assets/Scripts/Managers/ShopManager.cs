using UnityEngine;
using Solo.MOST_IN_ONE;
using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<UIParticle> bundleParticles;
    [SerializeField] private float bundleDelay = 0.8f;

    [SerializeField] private UIParticle noAdsRewardParticles;
    [SerializeField]
    private List<UIParticle> coinPackParticles;
    [SerializeField] private float rewardDelay = 0.8f;

    public void BuySuperBundle()
    {
        SoundManager.instance.PlaySound(SoundName.Coins);
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);

        // Coins
        DOVirtual.DelayedCall(0f, () =>
        {
            bundleParticles[0]?.Play();

            DOVirtual.DelayedCall(bundleDelay, () =>
            {
                SoundManager.instance.PlaySound(SoundName.CoinReach);
                CoinManager.instance.AddCoins(1200);
            }
             );
        }
        );

        // Undo
        DOVirtual.DelayedCall(0.50f, () =>
        {
            bundleParticles[1]?.Play();

            DOVirtual.DelayedCall(bundleDelay, () =>
            {
                SoundManager.instance.PlaySound(SoundName.CoinReach);
                BoosterManager.instance.AddUndo(3);
            }
            );

        }
        );

        // Magic
        DOVirtual.DelayedCall(1f, () =>
        {
            bundleParticles[2]?.Play();

            DOVirtual.DelayedCall(bundleDelay, () =>
            {
                SoundManager.instance.PlaySound(SoundName.CoinReach);
                BoosterManager.instance.AddMagic(3);
            }
            );
        }
        );

        // Shuffle
        DOVirtual.DelayedCall(1.5f, () =>
        {
            bundleParticles[3]?.Play();

            DOVirtual.DelayedCall(bundleDelay, () =>
            {
                SoundManager.instance.PlaySound(SoundName.CoinReach);
                BoosterManager.instance.AddShuffle(3);
            }
            );
        }
        );
    }

    public void BuyRemoveAds()
    {
        SoundManager.instance.PlaySound(SoundName.Coins);
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);

        if (noAdsRewardParticles != null)
        {
            noAdsRewardParticles.Play();
        }

        DOVirtual.DelayedCall(
            rewardDelay,
            () =>
            {
                SoundManager.instance.PlaySound(SoundName.CoinReach);
                CoinManager.instance.AddCoins(300);
            }
        );
    }

    public void Buy500Coins()
    {
        PlayCoinReward(500, coinPackParticles[0]);
    }

    public void Buy1500Coins()
    {
        PlayCoinReward(1500, coinPackParticles[1]);
    }

    public void Buy5000Coins()
    {
        PlayCoinReward(5000, coinPackParticles[2]);
    }

    void PlayCoinReward(int amount, UIParticle particle)
    {
        SoundManager.instance.PlaySound(SoundName.Coins);
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Success);

        if (particle != null)
        {
            particle.Play();
        }

        DOVirtual.DelayedCall(rewardDelay, () =>
        {
            SoundManager.instance.PlaySound(SoundName.CoinReach);
            CoinManager.instance.AddCoins(amount);
        }
        );
    }
}