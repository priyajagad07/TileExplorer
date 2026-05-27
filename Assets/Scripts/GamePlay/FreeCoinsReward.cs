using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FreeCoinsReward : MonoBehaviour
{
    [SerializeField]
    private UIParticle rewardParticles;

    [SerializeField]
    private Button watchButton;

    [SerializeField]
    private int rewardAmount = 50;

    [SerializeField]
    private float rewardDelay = 0.8f;

    public void ClaimFreeCoins()
    {
        watchButton.interactable = false;

        SoundManager.instance.PlaySound(SoundName.Coins);
        
        if (rewardParticles != null)
        {
            rewardParticles.Play();
        }

        DOVirtual.DelayedCall(
            rewardDelay,
            () =>
            {
                SoundManager.instance.PlaySound(SoundName.CoinReach);
                CoinManager.instance.AddCoins(rewardAmount);
                UIManager.Instance.HidePopup(ScreenType.FreeCoinsScreen);

                watchButton.interactable = true;
            }
        );
    }
}